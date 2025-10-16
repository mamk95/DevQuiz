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