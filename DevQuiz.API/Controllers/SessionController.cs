namespace DevQuiz.API.Controllers;
using System.Text.RegularExpressions;
using DevQuiz.API.Data;
using DevQuiz.API.Dtos;
using DevQuiz.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public partial class SessionController(QuizDbContext db) : ControllerBase
{
    private const int CookieTtlDays = 2;

    [HttpPost("start")]
    [ProducesResponseType(typeof(SessionStartedDto), 200)]
    [ProducesResponseType(typeof(SessionStartedDto), 400)]
    public async Task<ActionResult<SessionStartedDto>> Start([FromBody] StartSessionDto dto, CancellationToken ct)
    {
        var name = dto.Name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new SessionStartedDto { Success = false, Message = "Name is required" });

        if (!IsValidPhone(dto.Phone))
            return BadRequest(new SessionStartedDto { Success = false, Message = "Phone must be a valid international number" });

        var normalizedPhone = NormalizePhone(dto.Phone);

        var quiz = await db.Quizzes.FirstOrDefaultAsync(q => q.Difficulty == dto.Difficulty, ct);
        if (quiz == null)
            return BadRequest(new SessionStartedDto { Success = false, Message = "No quiz found for selected difficulty" });

        var existingParticipant = await db.Participants
            .Include(p => p.Sessions)
            .FirstOrDefaultAsync(p => p.Phone == normalizedPhone, ct);

        if (existingParticipant != null)
        {
            var hasCompletedThisQuiz = existingParticipant.Sessions
                .Any(s => s.QuizId == quiz.Id);

            if (hasCompletedThisQuiz)
            {
                return Ok(new SessionStartedDto
                {
                    Success = false,
                    Message = "You've already taken this quiz. Thanks!",
                });
            }

            var newSession = new Session
            {
                Id = Guid.NewGuid(),
                ParticipantId = existingParticipant.Id,
                CurrentQuestionIndex = 0,
                StartedAtUtc = DateTime.UtcNow,
                QuizId = quiz.Id,
            };

            db.Sessions.Add(newSession);
            await db.SaveChangesAsync(ct);

            Response.Cookies.Append("QuizSession", newSession.Id.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(CookieTtlDays),
            });

            return Ok(new SessionStartedDto { Success = true });
        }

        var participant = new Participant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Phone = normalizedPhone,
            AvatarUrl = dto.AvatarUrl,
            CreatedAtUtc = DateTime.UtcNow,
        };

        var session = new Session
        {
            Id = Guid.NewGuid(),
            ParticipantId = participant.Id,
            CurrentQuestionIndex = 0,
            StartedAtUtc = DateTime.UtcNow,
            QuizId = quiz.Id,
        };

        db.Participants.Add(participant);
        db.Sessions.Add(session);
        await db.SaveChangesAsync(ct);

        Response.Cookies.Append("QuizSession", session.Id.ToString(), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(CookieTtlDays),
        });

        return Ok(new SessionStartedDto { Success = true });
    }

    private static bool IsValidPhone(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return false;
        return PhoneRegex().IsMatch(phone);
    }

    private static string NormalizePhone(string phone)
    {
        return phone.Trim();
    }

    [GeneratedRegex(@"^\+\d{1,5}\d{4,15}$")]
    private static partial Regex PhoneRegex();
}