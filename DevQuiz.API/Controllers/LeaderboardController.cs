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
                AvatarUrl = x.participant.AvatarUrl ?? string.Empty,
            })
            .ToListAsync(ct);

        return Ok(entries);
    }
}