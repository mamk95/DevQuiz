namespace DevQuiz.API.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Progress
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public int QuestionId { get; set; }

    [Required]
    public DateTime StartAtUtc { get; set; }

    public int? DurationMs { get; set; }

    public int PenaltyMs { get; set; }

    public bool IsCorrect { get; set; }

    [ForeignKey(nameof(SessionId))]
    public Session? Session { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public Question? Question { get; set; }
}