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

        var quiz = await db.Quizzes.FirstOrDefaultAsync(q => q.Difficulty == dto.Difficulty, ct);
        if (quiz == null)
            return BadRequest(new SessionStartedDto { Success = false, Message = "No quiz found for selected difficulty" });

        var existingParticipant = await db.Participants
            .Include(p => p.Sessions)
            .FirstOrDefaultAsync(p => p.Phone == normalizedPhone, ct);

        var totalQuestions = await db.QuizQuestions
            .Where(qq => qq.QuizId == quiz.Id)
            .CountAsync(ct);

        if (totalQuestions == 0)
            return BadRequest(new SessionStartedDto { Success = false, Message = "Quiz has no questions configured" });
            

        if (existingParticipant != null)
        {
            var hasCompletedThisQuiz = existingParticipant.Sessions
                .Any(s => s.QuizId == quiz.Id && s.CompletedAtUtc != null);

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

            try
            {
                db.Sessions.Add(newSession);
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                return Ok(new SessionStartedDto
                {
                    Success = false,
                    Message = "You've already taken this quiz. Thanks!",
                });
            }

            Response.Cookies.Append("QuizSession", newSession.Id.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(CookieTtlDays),
            });

            return Ok(new SessionStartedDto { Success = true, TotalQuestions = totalQuestions });
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

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Ok(new SessionStartedDto
            {
                Success = false,
                Message = "You've already taken this quiz. Thanks!",
            });
        }

        Response.Cookies.Append("QuizSession", session.Id.ToString(), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(CookieTtlDays),
        });

        return Ok(new SessionStartedDto { Success = true, TotalQuestions = totalQuestions });
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
            return NoContent();
        }

        var sessionsQuery = db.Sessions.Include(s => s.Participant).Include(s => s.Progresses);
        var session = await sessionsQuery.FirstOrDefaultAsync(s => s.Id == sessionId.Value, ct);

        if (session == null || session.Participant == null)
        {

            InvalidateSessionCookie();
            return NoContent();
        }

        if (session.CompletedAtUtc != null)
        {
            InvalidateSessionCookie();
            return NoContent();
        }

        var totalQuestions = await db.QuizQuestions
            .Where(qq => qq.QuizId == session.QuizId)
            .CountAsync(ct);
        var answeredQuestions = session.Progresses.Count(p => p.IsCorrect);

        if (answeredQuestions >= totalQuestions)
        {
            InvalidateSessionCookie();
            return NoContent();
        }

        var totalTimeMs = session.Progresses.Sum(p => (p.DurationMs ?? 0) + p.PenaltyMs);

        var response = new ResumeSessionDto
        {
            QuestionIndex = session.CurrentQuestionIndex,
            Finished = false, // Since we checked and returned if answeredQuestions >= totalQuestions
            ParticipantName = session.Participant.Name,
            ParticipantPhone = session.Participant.Phone,
            TotalTimeMs = totalTimeMs,
            Success = true,
            TotalQuestions = totalQuestions,
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

    private void InvalidateSessionCookie()
    {
        Response.Cookies.Delete("QuizSession", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
        });
    }

    [HttpPost("submit-email")]
    [ProducesResponseType(typeof(SubmitEmailResultDto), 200)]
    [ProducesResponseType(typeof(SubmitEmailResultDto), 400)]
    public async Task<ActionResult<SubmitEmailResultDto>> SubmitEmail([FromBody] SubmitEmailDto dto, CancellationToken ct)
    {
        var email = dto.Email?.Trim().ToLowerInvariant();
        
        if (!IsValidEmail(email))
            return BadRequest(new SubmitEmailResultDto 
            { 
                Success = false, 
                Message = "Email is not valid", 
            });

        if (!Request.Cookies.TryGetValue("QuizSession", out var sessionIdStr) ||
            !Guid.TryParse(sessionIdStr, out var sessionId))
            return BadRequest(new SubmitEmailResultDto
            {
                Success = false,
                Message = "Session not found",
            });

        var session = await db.Sessions
            .Include(s => s.Participant)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

        if (session?.Participant == null)
            return BadRequest(new SubmitEmailResultDto 
            { 
                Success = false, 
                Message = "Session not found",
            });

        if (session.CompletedAtUtc == null)
            return BadRequest(new SubmitEmailResultDto
            {
                Success = false,
                Message = "Quiz must be completed before submitting email",
            });

        // Check if participant already has an email
        if (!string.IsNullOrEmpty(session.Participant.Email))
            return BadRequest(new SubmitEmailResultDto
            {
                Success = false,
                Message = "Email already submitted for this session",
            });

        var emailExists = await db.Participants
            .AnyAsync(p => p.Email == email, ct);

        if (emailExists)
        {
            return BadRequest(new SubmitEmailResultDto
            {
                Success = false,
                Message = "This email address is already registered",
            });
        }

        session.Participant.Email = email;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return BadRequest(new SubmitEmailResultDto
            {
                Success = false,
                Message = "This email address is already registered",
            });
        }

        return Ok(new SubmitEmailResultDto
        {
            Success = true,
            Message = "Email successfully registered for job opportunities",
        });
    }

    private static bool IsValidPhone(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return false;
        return PhoneRegex().IsMatch(phone);
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        
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