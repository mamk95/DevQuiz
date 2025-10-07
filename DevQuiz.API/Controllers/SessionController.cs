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
            AvatarUrl = dto.AvatarUrl,
            CreatedAtUtc = DateTime.UtcNow,
        };

        // Select quiz by difficulty
        var quizExists = await db.Quizzes.AnyAsync(q => q.Difficulty == dto.Difficulty, ct);
        if (!quizExists)
            return BadRequest(new SessionStartedDto { Success = false, Message = "No quiz found for selected difficulty" });

        var session = new Session
        {
            Id = Guid.NewGuid(),
            ParticipantId = participant.Id,
            CurrentQuestionIndex = 0,
            StartedAtUtc = DateTime.UtcNow,
            Difficulty = dto.Difficulty,
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
    [ProducesResponseType(typeof(ResumeSessionDto), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<ResumeSessionDto>> GetProgress(CancellationToken ct)
    {
        var sessionId = GetSessionIdFromCookie();
        if (sessionId == null)
        {

            InvalidateSessionCookie();
            return Unauthorized();
        }

        var sessionsQuery = db.Sessions.Include(s => s.Participant).Include(s => s.Progresses);
        var session = await sessionsQuery.FirstOrDefaultAsync(s => s.Id == sessionId.Value, ct);

        if (session == null || session.Participant == null)
        {

            InvalidateSessionCookie();
            return Unauthorized();
        }

        if (session.CompletedAtUtc != null)
        {
            InvalidateSessionCookie();
            return Unauthorized();
        }

        var totalQuestions = await db.Questions.CountAsync(ct);
        var answeredQuestions = session.Progresses.Count(p => p.IsCorrect);

        var totalTimeMs = session.Progresses.Sum(p => (p.DurationMs ?? 0) + p.PenaltyMs);

        var response = new ResumeSessionDto
        {
            QuestionIndex = session.CurrentQuestionIndex,
            Finished = answeredQuestions >= totalQuestions,
            ParticipantName = session.Participant.Name,
            ParticipantPhone = session.Participant.Phone,
            TotalTimeMs = totalTimeMs,
            Success = true,
        };

        if (answeredQuestions >= totalQuestions)
        {
            InvalidateSessionCookie();
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

    private void InvalidateSessionCookie()
    {
        Response.Cookies.Delete("QuizSession", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
        });
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