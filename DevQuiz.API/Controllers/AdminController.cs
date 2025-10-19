namespace DevQuiz.API.Controllers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DevQuiz.API.Data;
using DevQuiz.API.Dtos;
using DevQuiz.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class AdminController(QuizDbContext db, IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(AdminAuthResultDto), 200)]
    public IActionResult Login([FromBody] AdminLoginDto loginDto)
    {
        var adminPassword = configuration["AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            return StatusCode(500, new { message = "Server configuration error: AdminPassword not configured" });
        }

        if (loginDto.Password != adminPassword)
        {
            return Ok(new AdminAuthResultDto
            {
                Success = false,
                Message = "Invalid password"
            });
        }

        // Generate JWT token
        var jwtSecret = configuration["JwtSecret"];
        if (string.IsNullOrWhiteSpace(jwtSecret))
        {
            return StatusCode(500, new { message = "Server configuration error: JwtSecret not configured" });
        }

        var key = Encoding.ASCII.GetBytes(jwtSecret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Role, "Admin")
            ]),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new AdminAuthResultDto
        {
            Success = true,
            Token = tokenString
        });
    }

    [HttpGet("leaderboard")]
    [Authorize]
    [ProducesResponseType(typeof(List<AdminLeaderboardEntryDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<AdminLeaderboardEntryDto>>> GetLeaderboard(
        CancellationToken ct,
        [FromQuery] int limit = 100,
        [FromQuery] string? difficulty = null)
    {
        if (limit <= 0) limit = 100;
        if (limit > 1000) limit = 1000;

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
            .Select(x => new AdminLeaderboardEntryDto
            {
                ParticipantId = x.participant.Id,
                Name = x.participant.Name,
                Phone = x.participant.Phone,
                TotalMs = x.score.TotalMs,
                AvatarUrl = x.participant.AvatarUrl ?? string.Empty,
                Difficulty = x.session.Quiz != null ? x.session.Quiz.Difficulty ?? "Unknown" : "Unknown"
            })
            .ToListAsync(ct);

        return Ok(entries);
    }

    [HttpDelete("participant/{id}")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteParticipant(Guid id, CancellationToken ct)
    {
        // Use AsNoTracking for the existence check since we don't need to track changes
        var exists = await db.Participants.AsNoTracking().AnyAsync(p => p.Id == id, ct);

        if (!exists)
        {
            return NotFound(new { message = "Participant not found" });
        }

        // Delete participant only - cascade delete will handle Sessions, Scores, and Progress automatically
        // Per QuizDbContext configuration:
        // - Session has DeleteBehavior.Cascade from Participant
        // - Score has DeleteBehavior.Cascade from Session
        // - Progress has DeleteBehavior.Cascade from Session
        var participant = new Participant { Id = id, Name = string.Empty, Phone = string.Empty, AvatarUrl = string.Empty };
        db.Participants.Attach(participant);
        db.Participants.Remove(participant);

        await db.SaveChangesAsync(ct);

        return Ok(new { message = "Participant deleted successfully" });
    }
}
