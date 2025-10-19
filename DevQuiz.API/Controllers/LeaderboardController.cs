namespace DevQuiz.API.Controllers;
using DevQuiz.API.Data;
using DevQuiz.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController(QuizDbContext db) : ControllerBase
{
    [HttpGet("top")]
    [ProducesResponseType(typeof(List<LeaderboardEntryDto>), 200)]
    public async Task<ActionResult<List<LeaderboardEntryDto>>> GetTop(CancellationToken ct, [FromQuery] int limit = 10, [FromQuery] string? difficulty = null)
    {
        if (limit <= 0) limit = 10;
        if (limit > 100) limit = 100;

        var query = db.Scores
            .Join(
                db.Sessions.Include(s => s.Quiz),
                score => score.SessionId,
                session => session.Id,
                (score, session) => new { score, session })
            .Join(
                db.Participants,
                combined => combined.session.ParticipantId,
                participant => participant.Id,
                (combined, participant) => new { combined.score, combined.session, participant })
            .Where(x => x.session.CompletedAtUtc != null);

        if (!string.IsNullOrEmpty(difficulty))
        {
            query = query.Where(x => x.session.Quiz != null && x.session.Quiz.Difficulty == difficulty);
        }

        var entries = await query
            .OrderBy(x => x.score.TotalMs)
            .ThenBy(x => x.session.CompletedAtUtc)
            .Take(limit)
            .Select(x => new LeaderboardEntryDto
            {
                Name = x.participant.Name,
                TotalMs = x.score.TotalMs,
                AvatarUrl = x.participant.AvatarUrl ?? string.Empty,
            })
            .ToListAsync(ct);

        return Ok(entries);
    }

    [HttpGet("ongoing")]
    [ProducesResponseType(typeof(List<OngoingParticipantDto>), 200)]
    public async Task<ActionResult<List<OngoingParticipantDto>>> GetOngoing(CancellationToken ct, [FromQuery] string? difficulty = null)
    {
        var cutoffTime = DateTime.UtcNow.AddSeconds(-100); // 100 seconds ago

        var query = db.Sessions
            .Include(s => s.Participant)
            .Include(s => s.Quiz)
            .Where(s => s.CompletedAtUtc == null && s.StartedAtUtc >= cutoffTime);

        if (!string.IsNullOrEmpty(difficulty))
        {
            query = query.Where(s => s.Quiz != null && s.Quiz.Difficulty == difficulty);
        }

        var sessions = await query.ToListAsync(ct);

        if (sessions.Count == 0)
            return Ok(new List<OngoingParticipantDto>());

        // Pre-fetch question counts for all unique quizzes to avoid N+1 queries
        var uniqueQuizIds = sessions
            .Where(s => s.Quiz != null)
            .Select(s => s.QuizId)
            .Distinct()
            .ToList();

        var questionCounts = await db.QuizQuestions
            .Where(qq => uniqueQuizIds.Contains(qq.QuizId))
            .GroupBy(qq => qq.QuizId)
            .Select(g => new { QuizId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.QuizId, x => x.Count, ct);

        // Fetch last activity and penalties in a single query
        var sessionIds = sessions.Select(s => s.Id).ToList();
        var progressData = await db.Progresses
            .Where(p => sessionIds.Contains(p.SessionId))
            .GroupBy(p => p.SessionId)
            .Select(g => new
            {
                SessionId = g.Key,
                LastActivityTime = g.Max(p => p.StartAtUtc),
                TotalPenaltyMs = g.Sum(p => p.PenaltyMs)
            })
            .ToDictionaryAsync(x => x.SessionId, ct);

        var ongoingParticipants = new List<OngoingParticipantDto>();

        foreach (var session in sessions)
        {
            if (session.Participant == null || session.Quiz == null)
                continue;

            // Get progress data from pre-fetched dictionary
            var lastActivityTime = session.StartedAtUtc;
            var totalPenaltyMs = 0;

            if (progressData.TryGetValue(session.Id, out var data))
            {
                lastActivityTime = data.LastActivityTime;
                totalPenaltyMs = data.TotalPenaltyMs;
            }

            if (DateTime.UtcNow - lastActivityTime > TimeSpan.FromSeconds(100))
                continue;

            var totalQuestions = questionCounts.GetValueOrDefault(session.QuizId, 0);

            ongoingParticipants.Add(new OngoingParticipantDto
            {
                SessionId = session.Id.ToString(),
                Name = session.Participant.Name,
                AvatarUrl = session.Participant.AvatarUrl ?? string.Empty,
                Difficulty = session.Quiz.Difficulty ?? "Unknown",
                StartedAtMs = new DateTimeOffset(session.StartedAtUtc, TimeSpan.Zero).ToUnixTimeMilliseconds(),
                LastActivityMs = new DateTimeOffset(lastActivityTime, TimeSpan.Zero).ToUnixTimeMilliseconds(),
                CurrentQuestionIndex = session.CurrentQuestionIndex,
                TotalQuestions = totalQuestions,
                TotalPenaltyMs = totalPenaltyMs,
            });
        }

        return Ok(ongoingParticipants);
    }

    [HttpGet("my-score")]
    [ProducesResponseType(typeof(LeaderboardMyScoreDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<LeaderboardMyScoreDto>> GetMyScore(CancellationToken ct, [FromQuery] string? difficulty = null)
    {
        var sessionId = GetSessionIdFromCookie();
        if (sessionId == null)
            return Unauthorized();

        var result = await db.Scores
            .Join(
                db.Sessions.Include(s => s.Quiz),
                score => score.SessionId,
                session => session.Id,
                (score, session) => new { score, session })
            .Join(
                db.Participants,
                combined => combined.session.ParticipantId,
                participant => participant.Id,
                (combined, participant) => new { combined.score, combined.session, participant })
            .Where(x => x.session.Id == sessionId.Value && x.session.CompletedAtUtc != null)
            .Select(x => new
            {
                x.participant.Name,
                x.score.TotalMs,
                CompletedAt = x.session.CompletedAtUtc,
                QuizId = x.session.QuizId,
                QuizDifficulty = x.session.Quiz != null ? x.session.Quiz.Difficulty : null
            })
            .FirstOrDefaultAsync(ct);

        if (result == null)
            return NotFound();

        // Position is quiz-specific if difficulty provided, otherwise uses session's quiz
        var positionQuery = db.Scores
            .Join(
                db.Sessions.Include(s => s.Quiz),
                score => score.SessionId,
                session => session.Id,
                (score, session) => new { score, session })
            .Where(x => x.session.CompletedAtUtc != null && x.score.TotalMs < result.TotalMs);

        if (!string.IsNullOrEmpty(difficulty))
        {
            positionQuery = positionQuery.Where(x => x.session.Quiz != null && x.session.Quiz.Difficulty == difficulty);
        }
        else if (result.QuizDifficulty != null)
        {
            // Default to session's quiz for position calculation
            positionQuery = positionQuery.Where(x => x.session.Quiz != null && x.session.Quiz.Difficulty == result.QuizDifficulty);
        }

        var position = await positionQuery.CountAsync(ct) + 1;

        var totalParticipantsQuery = db.Sessions.Include(s => s.Quiz)
            .Where(s => s.CompletedAtUtc != null);

        if (!string.IsNullOrEmpty(difficulty))
        {
            totalParticipantsQuery = totalParticipantsQuery.Where(s => s.Quiz != null && s.Quiz.Difficulty == difficulty);
        }
        else if (result.QuizDifficulty != null)
        {
            totalParticipantsQuery = totalParticipantsQuery.Where(s => s.Quiz != null && s.Quiz.Difficulty == result.QuizDifficulty);
        }

        var totalParticipants = await totalParticipantsQuery.CountAsync(ct);

        var response = new LeaderboardMyScoreDto
        {
            Name = result.Name,
            TotalMs = result.TotalMs,
            Position = position,
            TotalParticipants = totalParticipants,
            CompletedAtUtc = result.CompletedAt
        };

        return Ok(response);
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
}