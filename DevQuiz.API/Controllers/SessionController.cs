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
    [ProducesResponseType(typeof(SessionStartedDto), 401)]
    public async Task<ActionResult<SessionStartedDto>> Start([FromBody] StartSessionDto dto, CancellationToken ct)
    {
        var name = dto.Name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new SessionStartedDto { Success = false, Message = "Name is required" });

        if (!IsValidPhone(dto.Phone))
            return BadRequest(new SessionStartedDto { Success = false, Message = "Phone must be a valid international number" });

        var normalizedPhone = NormalizePhone(dto.Phone);

        var existingParticipant = await db.Participants
            .FirstOrDefaultAsync(p => p.Phone == normalizedPhone, ct);

        if (existingParticipant != null)
        {
            return Ok(new SessionStartedDto
            {
                Success = false,
                Message = "You've already participated. Thanks!",
            });
        }

        var participant = new Participant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Phone = normalizedPhone,
            CreatedAtUtc = DateTime.UtcNow,
        };

        var session = new Session
        {
            Id = Guid.NewGuid(),
            ParticipantId = participant.Id,
            CurrentQuestionIndex = 0,
            StartedAtUtc = DateTime.UtcNow,
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

    [HttpGet("resume")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<object>> GetProgress(CancellationToken ct)
    {
        var sessionId = GetSessionIdFromCookie();
        if (sessionId == null)
        {
            // Invalid or missing cookie - delete it to ensure clean state
            Response.Cookies.Delete("QuizSession", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
            });
            return Unauthorized();
        }

        var session = await db.Sessions
            .Include(s => s.Participant)
            .FirstOrDefaultAsync(s => s.Id == sessionId.Value, ct);

        if (session == null || session.Participant == null)
        {
            // Session doesn't exist in database - delete the invalid cookie
            Response.Cookies.Delete("QuizSession", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
            });
            return Unauthorized();
        }

        var totalQuestions = await db.Questions.CountAsync(ct);
        var answeredQuestions = await db.Progresses
            .Where(p => p.SessionId == sessionId.Value && p.IsCorrect)
            .CountAsync(ct);

        var response = new
        {
            questionIndex = session.CurrentQuestionIndex,
            finished = answeredQuestions >= totalQuestions,
            participantName = session.Participant.Name,
            participantPhone = session.Participant.Phone,
            totalTimeMs = (DateTime.UtcNow - session.StartedAtUtc).TotalMilliseconds,
            success = true,
        };

        // If session is finished, delete the cookie
        if (answeredQuestions >= totalQuestions)
        {
            Response.Cookies.Delete("QuizSession", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
            });
        }

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