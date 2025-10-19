namespace DevQuiz.API.Controllers;
using System.Text.Json;
using DevQuiz.API.Data;
using DevQuiz.API.Dtos;
using DevQuiz.API.Entities;
using DevQuiz.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class QuizController(
    QuizDbContext db,
    ILogger<QuizController> logger,
    IQuizHubService quizHubService,
    IServiceScopeFactory serviceScopeFactory) : ControllerBase
{
    private const int WrongAnswerPenaltyMs = 10000; // milliseconds
    private const int SkipPenaltyMs = 60000; // 60 seconds

    [HttpGet("current")]
    [ProducesResponseType(typeof(CurrentQuestionDto), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<CurrentQuestionDto>> GetCurrent(CancellationToken ct)
    {
        var sessionId = GetSessionIdFromCookie();
        if (sessionId == null)
            return Unauthorized();

        var session = await db.Sessions
            .Include(s => s.Progresses)
            .FirstOrDefaultAsync(s => s.Id == sessionId.Value, ct);

        if (session == null)
            return Unauthorized();

        if (session.CompletedAtUtc != null)
        {
            var totalMs = await db.Scores
                .Where(s => s.SessionId == sessionId.Value)
                .Select(s => s.TotalMs)
                .FirstOrDefaultAsync(ct);

            return Ok(new CurrentQuestionDto
            {
                Done = true,
                TotalMs = totalMs,
                SessionStartedAtUtc = session.StartedAtUtc,
            });
        }

        var quizQuestion = await db.QuizQuestions
            .Include(qq => qq.Question)
            .Where(qq => qq.QuizId == session.QuizId && qq.Sequence == session.CurrentQuestionIndex + 1)
            .FirstOrDefaultAsync(ct);

        var currentQuestion = quizQuestion?.Question;

        if (currentQuestion == null)
        {
            var totalMs = session.Progresses.Sum(p => (p.DurationMs ?? 0) + p.PenaltyMs);
            return Ok(new CurrentQuestionDto
            {
                Done = true,
                TotalMs = totalMs,
                SessionStartedAtUtc = session.StartedAtUtc,
            });
        }

        var progress = await db.Progresses
            .FirstOrDefaultAsync(p => p.SessionId == sessionId.Value && p.QuestionId == currentQuestion.Id, ct);

        if (progress == null)
        {
            progress = new Progress
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId.Value,
                QuestionId = currentQuestion.Id,
                StartAtUtc = DateTime.UtcNow,
                PenaltyMs = 0,
                IsCorrect = false,
            };
            db.Progresses.Add(progress);
            await db.SaveChangesAsync(ct);
        }

        var response = new CurrentQuestionDto
        {
            Done = false,
            QuestionIndex = session.CurrentQuestionIndex,
            Type = currentQuestion.Type == QuestionType.MultipleChoice ? "MC" : "CodeFix",
            Prompt = currentQuestion.Prompt,
            SessionStartedAtUtc = session.StartedAtUtc,
        };

        if (currentQuestion.Type == QuestionType.MultipleChoice && !string.IsNullOrEmpty(currentQuestion.ChoicesJson))
        {
            var choices = JsonSerializer.Deserialize<List<string>>(currentQuestion.ChoicesJson) ?? [];
            response.Choices = ShuffleChoices(choices);
        }
        else if (currentQuestion.Type == QuestionType.CodeFix && !string.IsNullOrEmpty(currentQuestion.ChoicesJson))
        {
            var codeFixData = JsonSerializer.Deserialize<Dictionary<string, string>>(currentQuestion.ChoicesJson);
            if (codeFixData != null)
            {
                response.InitialCode = codeFixData.GetValueOrDefault("initialCode");
                response.TestCode = codeFixData.GetValueOrDefault("testCode");
            }
        }

        return Ok(response);
    }

    [HttpPost("answer")]
    [ProducesResponseType(typeof(AnswerResultDto), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<AnswerResultDto>> SubmitAnswer([FromBody] SubmitAnswerDto dto, CancellationToken ct)
    {
        var sessionId = GetSessionIdFromCookie();
        if (sessionId == null)
            return Unauthorized();

        using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var session = await db.Sessions
                .Include(s => s.Progresses)
                .FirstOrDefaultAsync(s => s.Id == sessionId.Value, ct);

            if (session == null)
            {
                await transaction.RollbackAsync(ct);
                return Unauthorized();
            }

            if (session.CompletedAtUtc != null)
            {
                await transaction.RollbackAsync(ct);
                return BadRequest(new AnswerResultDto { Correct = false });
            }

            var quizQuestion = await db.QuizQuestions
                .Include(qq => qq.Question)
                .Where(qq => qq.QuizId == session.QuizId && qq.Sequence == session.CurrentQuestionIndex + 1)
                .FirstOrDefaultAsync(ct);

            var currentQuestion = quizQuestion?.Question;

            if (currentQuestion == null)
            {
                await transaction.RollbackAsync(ct);
                return BadRequest(new AnswerResultDto { Correct = false });
            }

            var progress = await db.Progresses
                .FirstOrDefaultAsync(p => p.SessionId == sessionId.Value && p.QuestionId == currentQuestion.Id, ct);

            if (progress == null)
            {
                await transaction.RollbackAsync(ct);
                return BadRequest(new AnswerResultDto { Correct = false });
            }

            var totalPenaltyMs = await db.Progresses
                .Where(p => p.SessionId == sessionId.Value)
                .SumAsync(p => p.PenaltyMs, ct);

            if (progress.IsCorrect)
            {
                await transaction.RollbackAsync(ct);
                var wasLastQuestion = !await db.QuizQuestions.AnyAsync(qq => qq.QuizId == session.QuizId && qq.Sequence == session.CurrentQuestionIndex + 2, ct);

                if (wasLastQuestion)
                {
                    var totalMs = session.Progresses.Sum(p => (p.DurationMs ?? 0) + p.PenaltyMs);

                    return Ok(new AnswerResultDto
                    {
                        Correct = true,
                        QuizCompleted = true,
                        TotalMs = totalMs,
                        TotalPenaltyMs = totalPenaltyMs,
                    });
                }

                return Ok(new AnswerResultDto { Correct = true, TotalPenaltyMs = totalPenaltyMs });
            }

            var answerText = dto.AnswerText?.Trim() ?? "";
            var isCorrect = string.Equals(answerText, currentQuestion.CorrectAnswer, StringComparison.Ordinal);

            if (!isCorrect)
            {
                progress.PenaltyMs += WrongAnswerPenaltyMs;
                await db.SaveChangesAsync(ct);

                var newTotalPenaltyMs = await db.Progresses
                    .Where(p => p.SessionId == sessionId.Value)
                    .SumAsync(p => p.PenaltyMs, ct);

                await transaction.CommitAsync(ct);

                // Send progress update to show penalty immediately (don't await to avoid blocking response)
                _ = Task.Run(() => SendProgressUpdateAsync(sessionId.Value, CancellationToken.None));

                return Ok(new AnswerResultDto
                {
                    Correct = false,
                    PenaltyMsAdded = WrongAnswerPenaltyMs,
                    TotalPenaltyMs = newTotalPenaltyMs
                });
            }

            var durationMs = (int)(DateTime.UtcNow - progress.StartAtUtc).TotalMilliseconds;
            progress.IsCorrect = true;
            progress.DurationMs = durationMs;

            session.CurrentQuestionIndex++;

            var nextQuestion = await db.QuizQuestions
                .AnyAsync(qq => qq.QuizId == session.QuizId && qq.Sequence == session.CurrentQuestionIndex + 1, ct);

            if (!nextQuestion)
            {
                var totalMs = 0;
                var allProgresses = await db.Progresses
                    .Where(p => p.SessionId == sessionId.Value)
                    .ToListAsync(ct);

                foreach (var p in allProgresses)
                {
                    totalMs += (p.DurationMs ?? 0) + p.PenaltyMs;
                }

                session.CompletedAtUtc = DateTime.UtcNow;

                var score = new Score
                {
                    SessionId = sessionId.Value,
                    TotalMs = totalMs,
                };
                db.Scores.Add(score);

                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                // Send completion notification (don't await to avoid blocking response)
                _ = Task.Run(() => SendCompletionNotificationAsync(sessionId.Value, totalMs, CancellationToken.None));

                return Ok(new AnswerResultDto
                {
                    Correct = true,
                    QuizCompleted = true,
                    TotalMs = totalMs,
                    TotalPenaltyMs = totalPenaltyMs,
                });
            }

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            // Send progress update (don't await to avoid blocking response)
            _ = Task.Run(() => SendProgressUpdateAsync(sessionId.Value, CancellationToken.None));

            return Ok(new AnswerResultDto
            {
                Correct = true,
                TotalPenaltyMs = totalPenaltyMs,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing answer");
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    [HttpPost("skip")]
    [ProducesResponseType(typeof(SkipResultDto), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<SkipResultDto>> SkipQuestion(CancellationToken ct)
    {
        var sessionId = GetSessionIdFromCookie();
        if (sessionId == null)
            return Unauthorized();

        using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var session = await db.Sessions
                .Include(s => s.Progresses)
                .FirstOrDefaultAsync(s => s.Id == sessionId.Value, ct);

            if (session == null)
            {
                await transaction.RollbackAsync(ct);
                return Unauthorized();
            }

            if (session.CompletedAtUtc != null)
            {
                await transaction.RollbackAsync(ct);
                return BadRequest(new SkipResultDto 
                { 
                    Success = false, 
                    Message = "Quiz already completed",
                    PenaltyMs = 0,
                    QuizCompleted = false 
                });
            }

            var quizQuestion = await db.QuizQuestions
                .Include(qq => qq.Question)
                .Where(qq => qq.QuizId == session.QuizId && qq.Sequence == session.CurrentQuestionIndex + 1)
                .FirstOrDefaultAsync(ct);

            var currentQuestion = quizQuestion?.Question;

            if (currentQuestion == null)
            {
                await transaction.RollbackAsync(ct);
                return BadRequest(new SkipResultDto 
                { 
                    Success = false, 
                    Message = "Question not found",
                    PenaltyMs = 0,
                    QuizCompleted = false 
                });
            }

            var progress = await db.Progresses
                .FirstOrDefaultAsync(p => p.SessionId == sessionId.Value && p.QuestionId == currentQuestion.Id, ct);

            if (progress == null)
            {
                await transaction.RollbackAsync(ct);
                return BadRequest(new SkipResultDto 
                { 
                    Success = false, 
                    Message = "Progress not found",
                    PenaltyMs = 0,
                    QuizCompleted = false 
                });
            }

            var actualDurationMs = (int)(DateTime.UtcNow - progress.StartAtUtc).TotalMilliseconds;
            progress.IsCorrect = false;
            progress.DurationMs = actualDurationMs;
            progress.PenaltyMs += SkipPenaltyMs;

            session.CurrentQuestionIndex++;

            var nextQuestion = await db.QuizQuestions
                .AnyAsync(qq => qq.QuizId == session.QuizId && qq.Sequence == session.CurrentQuestionIndex + 1, ct);

            if (!nextQuestion)
            {
                // Quiz completed
                var totalMs = 0;
                var allProgresses = await db.Progresses
                    .Where(p => p.SessionId == sessionId.Value)
                    .ToListAsync(ct);

                foreach (var p in allProgresses)
                {
                    totalMs += (p.DurationMs ?? 0) + p.PenaltyMs;
                }

                session.CompletedAtUtc = DateTime.UtcNow;

                var score = new Score
                {
                    SessionId = sessionId.Value,
                    TotalMs = totalMs
                };

                db.Scores.Add(score);

                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                // Send completion notification (don't await to avoid blocking response)
                _ = Task.Run(() => SendCompletionNotificationAsync(sessionId.Value, totalMs, CancellationToken.None));

                return Ok(new SkipResultDto
                {
                    Success = true,
                    PenaltyMs = SkipPenaltyMs,
                    QuizCompleted = true,
                    TotalMs = totalMs
                });
            }

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            // Send progress update (don't await to avoid blocking response)
            _ = Task.Run(() => SendProgressUpdateAsync(sessionId.Value, CancellationToken.None));

            return Ok(new SkipResultDto
            {
                Success = true,
                PenaltyMs = SkipPenaltyMs,
                QuizCompleted = false
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error skipping question");
            await transaction.RollbackAsync(ct);
            return BadRequest(new SkipResultDto 
            { 
                Success = false, 
                Message = "An error occurred while skipping the question",
                PenaltyMs = 0,
                QuizCompleted = false 
            });
        }
    }

    private Guid? GetSessionIdFromCookie()
    {
        if (Request.Cookies.TryGetValue("QuizSession", out var sessionIdStr) &&
            Guid.TryParse(sessionIdStr, out var sessionId))
        {
            return sessionId;
        }
        return null;
    }

    private static List<string> ShuffleChoices(List<string> choices)
    {
        var shuffled = new List<string>(choices);
        var random = new Random();

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        return shuffled;
    }

    private async Task SendProgressUpdateAsync(Guid sessionId, CancellationToken ct)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var scopedDb = scope.ServiceProvider.GetRequiredService<QuizDbContext>();

            var session = await scopedDb.Sessions
                .Include(s => s.Participant)
                .Include(s => s.Quiz)
                .Include(s => s.Progresses)
                .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

            if (session?.Participant == null || session.Quiz == null)
            {
                logger.LogWarning("Session {SessionId} not found or missing participant/quiz for progress update", sessionId);
                return;
            }

            var totalQuestions = await scopedDb.QuizQuestions
                .Where(qq => qq.QuizId == session.QuizId)
                .CountAsync(ct);

            var totalPenaltyMs = session.Progresses.Sum(p => p.PenaltyMs);

            var ongoingParticipant = new OngoingParticipantDto
            {
                SessionId = session.Id.ToString(),
                Name = session.Participant.Name,
                AvatarUrl = session.Participant.AvatarUrl ?? string.Empty,
                Difficulty = session.Quiz.Difficulty ?? "Unknown",
                StartedAtMs = new DateTimeOffset(session.StartedAtUtc, TimeSpan.Zero).ToUnixTimeMilliseconds(),
                LastActivityMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                CurrentQuestionIndex = session.CurrentQuestionIndex,
                TotalQuestions = totalQuestions,
                TotalPenaltyMs = totalPenaltyMs,
            };

            await quizHubService.SendParticipantProgressAsync(ongoingParticipant);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending progress update for session {SessionId}", sessionId);
        }
    }

    private async Task SendCompletionNotificationAsync(Guid sessionId, int totalMs, CancellationToken ct)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var scopedDb = scope.ServiceProvider.GetRequiredService<QuizDbContext>();

            var session = await scopedDb.Sessions
                .Include(s => s.Participant)
                .Include(s => s.Quiz)
                .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

            if (session?.Participant == null || session.Quiz == null)
            {
                logger.LogWarning("Session {SessionId} not found or missing participant/quiz", sessionId);
                return;
            }

            if (session.CompletedAtUtc == null)
            {
                logger.LogWarning("Session {SessionId} for {Name} has no completion time", sessionId, session.Participant.Name);
                return;
            }

            // Calculate ranking (must account for tie-breaking by CompletedAtUtc)
            var ranking = await scopedDb.Scores
                .Join(scopedDb.Sessions, score => score.SessionId, s => s.Id, (score, s) => new { score, s })
                .Where(x => x.s.QuizId == session.QuizId &&
                           (x.score.TotalMs < totalMs ||
                            (x.score.TotalMs == totalMs && x.s.CompletedAtUtc < session.CompletedAtUtc)))
                .CountAsync(ct) + 1;

            var isTopThree = ranking <= 3;
            var isOnLeaderboard = ranking <= 10;

            var completion = new ParticipantCompletionDto
            {
                SessionId = session.Id.ToString(),
                Name = session.Participant.Name,
                AvatarUrl = session.Participant.AvatarUrl ?? string.Empty,
                Difficulty = session.Quiz.Difficulty ?? "Unknown",
                TotalMs = totalMs,
                Ranking = ranking,
                IsTopThree = isTopThree,
                IsOnLeaderboard = isOnLeaderboard,
            };

            await quizHubService.SendParticipantCompletedAsync(completion);

            // Also send updated leaderboard
            var leaderboard = await scopedDb.Scores
                .Join(scopedDb.Sessions, score => score.SessionId, s => s.Id, (score, s) => new { score, s })
                .Join(scopedDb.Participants, x => x.s.ParticipantId, p => p.Id, (x, p) => new { x.score, x.s, p })
                .Where(x => x.s.QuizId == session.QuizId && x.s.CompletedAtUtc != null)
                .OrderBy(x => x.score.TotalMs)
                .ThenBy(x => x.s.CompletedAtUtc)
                .Take(10)
                .Select(x => new LeaderboardEntryDto
                {
                    Name = x.p.Name,
                    TotalMs = x.score.TotalMs,
                    AvatarUrl = x.p.AvatarUrl ?? string.Empty,
                })
                .ToListAsync(ct);

            await quizHubService.SendLeaderboardUpdateAsync(session.Quiz.Difficulty ?? "Unknown", leaderboard);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending completion notification for session {SessionId}", sessionId);
        }
    }
}