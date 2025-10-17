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


    [HttpGet("most-recent")]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(List<MostRecentParticipantDto>), 200)]
    public async Task<ActionResult<List<MostRecentParticipantDto>>> GetMostRecent(CancellationToken ct, [FromQuery] int limit = 10, [FromQuery] string? difficulty = null)
    {
        // Should be above 1x and less than 1.5x polling interval to handle high latency
        // With 10s frontend polling, less than 15s ensures participants appear only once in one polling cycle
        const int IncludeCompletedParticipantsWithinTimeSlotSeconds = 13;

        // Prevents counting participants who are idle for too long
        const int IdleThresholdMinutes = 10;


        if (limit <= 0) limit = 10;
        if (limit > 100) limit = 100;

        var quizId = await db.Quizzes
            .Where(q => q.Difficulty == difficulty)
            .Select(q => q.Id)
            .FirstOrDefaultAsync(ct);
        
        if (quizId == 0 && !string.IsNullOrEmpty(difficulty))
        {
            return NotFound();
        }

        // Get recent sessions with participant info in one query
        var recentSessions = await db.Sessions
            .Where(s => s.QuizId == quizId)
            .Where(s => s.StartedAtUtc >= DateTime.UtcNow.AddMinutes(-IdleThresholdMinutes))
            .Where(s => s.CompletedAtUtc == null || s.CompletedAtUtc >= DateTime.UtcNow.AddSeconds(-IncludeCompletedParticipantsWithinTimeSlotSeconds))
            .OrderByDescending(s => s.StartedAtUtc)
            .Take(limit)
            .Include(s => s.Participant)
            .ToListAsync(ct);

        var sessionIds = recentSessions.Select(s => s.Id).ToList();

        // Get all penalties in one query
        var penalties = await db.Progresses
            .Where(p => sessionIds.Contains(p.SessionId))
            .GroupBy(p => p.SessionId)
            .Select(g => new { SessionId = g.Key, TotalPenalty = g.Sum(p => p.PenaltyMs) })
            .ToDictionaryAsync(x => x.SessionId, x => x.TotalPenalty, ct);

        // Get all scores for completed sessions in one query
        var completedSessionIds = recentSessions
            .Where(s => s.CompletedAtUtc != null)
            .Select(s => s.Id)
            .ToList();

        var scores = await db.Scores
            .Where(s => completedSessionIds.Contains(s.SessionId))
            .ToDictionaryAsync(s => s.SessionId, s => s.TotalMs, ct);

        // Calculate positions for all completed sessions efficiently
        var positionsDict = new Dictionary<Guid, int>();
        if (scores.Count > 0)
        {
            // Fetch all scores once and sort them
            var allScoresForQuiz = await db.Scores
                .Join(db.Sessions, s => s.SessionId, sess => sess.Id, (s, sess) => new { s.SessionId, s.TotalMs, sess.QuizId, sess.CompletedAtUtc })
                .Where(x => x.CompletedAtUtc != null && x.QuizId == quizId)
                .OrderBy(x => x.TotalMs)
                .Select(x => new { x.SessionId, x.TotalMs })
                .ToListAsync(ct);

            // Assign positions based on sorted order
            for (int i = 0; i < allScoresForQuiz.Count; i++)
            {
                var score = allScoresForQuiz[i];
                if (scores.ContainsKey(score.SessionId))
                {
                    positionsDict[score.SessionId] = i + 1;
                }
            }
        }

        var result = recentSessions
            .Where(s => s.Participant != null)
            .Select(session =>
            {
                var totalPenalty = penalties.GetValueOrDefault(session.Id, 0);
                
                // For completed sessions, use the final score; for in-progress, calculate elapsed time
                int totalMs;
                if (session.CompletedAtUtc != null && scores.TryGetValue(session.Id, out var finalScore))
                {
                    totalMs = finalScore; // Use final score from Scores table
                }
                else
                {
                    totalMs = (int)(DateTime.UtcNow - session.StartedAtUtc).TotalMilliseconds + totalPenalty;
                }
                
                int? position = null;
                if (session.CompletedAtUtc != null && positionsDict.TryGetValue(session.Id, out var pos))
                {
                    position = pos;
                }

                return new MostRecentParticipantDto
                {
                    Name = session.Participant!.Name,
                    TotalMs = totalMs,
                    AvatarUrl = session.Participant.AvatarUrl ?? string.Empty,
                    CompletedAt = session.CompletedAtUtc,
                    Position = position
                };
            }).ToList();

        return Ok(result);
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