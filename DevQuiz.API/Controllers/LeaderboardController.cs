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
    public async Task<ActionResult<List<LeaderboardEntryDto>>> GetTop(CancellationToken ct, [FromQuery] int limit = 10)
    {
        if (limit <= 0) limit = 10;
        if (limit > 100) limit = 100;

        var entries = await db.Scores
            .Join(
                db.Sessions,
                score => score.SessionId,
                session => session.Id,
                (score, session) => new { score, session })
            .Join(
                db.Participants,
                combined => combined.session.ParticipantId,
                participant => participant.Id,
                (combined, participant) => new { combined.score, combined.session, participant })
            .Where(x => x.session.CompletedAtUtc != null)
            .OrderBy(x => x.score.TotalMs)
            .ThenBy(x => x.session.CompletedAtUtc)
            .Take(limit)
            .Select(x => new LeaderboardEntryDto
            {
                Name = x.participant.Name,
                TotalMs = x.score.TotalMs,
            })
            .ToListAsync(ct);

        return Ok(entries);
    }

    [HttpGet("my-score")]
    [ProducesResponseType(typeof(LeaderboardMyScoreDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<LeaderboardMyScoreDto>> GetMyScore(CancellationToken ct)
    {
        var sessionId = GetSessionIdFromCookie();
        if (sessionId == null)
            return Unauthorized();

        var result = await db.Scores
            .Join(
                db.Sessions,
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
                CompletedAt = x.session.CompletedAtUtc
            })
            .FirstOrDefaultAsync(ct);

        if (result == null)
            return NotFound();

        // Calculate position and total participants with a single efficient query using joins
        var completedScores = await db.Scores
            .Join(
                db.Sessions,
                score => score.SessionId,
                session => session.Id,
                (score, session) => new { score, session })
            .Where(x => x.session.CompletedAtUtc != null)
            .Select(x => x.score.TotalMs)
            .ToListAsync(ct);

        var position = completedScores.Count(ms => ms < result.TotalMs) + 1;
        var totalParticipants = completedScores.Count;

        var response = new
        {
            name = result.Name,
            totalMs = result.TotalMs,
            position,
            totalParticipants,
            completedAt = result.CompletedAt
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