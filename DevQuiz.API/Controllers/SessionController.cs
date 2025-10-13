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

    [HttpPost("submit-email")]
    [ProducesResponseType(typeof(SubmitEmailResultDto), 200)]
    [ProducesResponseType(typeof(SubmitEmailResultDto), 400)]
    public async Task<ActionResult<SubmitEmailResultDto>> SubmitEmail([FromBody] SubmitEmailDto dto, CancellationToken ct)
    {
        var email = dto.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!IsValidEmail(email))
            return BadRequest(new SubmitEmailResultDto 
            { 
                Success = false, 
                Message = "Email is not valid" 
            });

        var session = await db.Sessions
            .Include(s => s.Participant)
            .FirstOrDefaultAsync(s => s.Id.ToString() == Request.Cookies["QuizSession"], ct);

        if (session?.Participant == null)
            return BadRequest(new SubmitEmailResultDto 
            { 
                Success = false, 
                Message = "Session not found" 
            });

        // Check if quiz is completed
        if (session.CompletedAtUtc == null)
            return BadRequest(new SubmitEmailResultDto
            {
                Success = false,
                Message = "Quiz must be completed before submitting email"
            });

        // Check if participant already has an email
        if (!string.IsNullOrEmpty(session.Participant.Email))
            return BadRequest(new SubmitEmailResultDto
            {
                Success = false,
                Message = "Email already submitted for this session"
            });

        // Check for duplicate email (case-insensitive)
        var emailExists = await db.Participants
            .AnyAsync(p => p.Email != null && p.Email.ToLower() == email, ct);

        if (emailExists)
        {
            return BadRequest(new SubmitEmailResultDto
            {
                Success = false,
                Message = "This email address is already registered"
            });
        }

        session.Participant.Email = email;
        await db.SaveChangesAsync(ct);

        return Ok(new SubmitEmailResultDto
        {
            Success = true,
            Message = "Email successfully registered for job opportunities"
        });
    }

    private static bool IsValidPhone(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return false;
        return PhoneRegex().IsMatch(phone);
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizePhone(string phone)
    {
        return phone.Trim();
    }

    [GeneratedRegex(@"^\+\d{1,5}\d{4,15}$")]
    private static partial Regex PhoneRegex();
}