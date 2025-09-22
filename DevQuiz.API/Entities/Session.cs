namespace DevQuiz.API.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Session
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ParticipantId { get; set; }

    public int CurrentQuestionIndex { get; set; }

    [Required]
    public DateTime StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    [ForeignKey(nameof(ParticipantId))]
    public Participant? Participant { get; set; }

    public ICollection<Progress> Progresses { get; set; } = [];

    public Score? Score { get; set; }
}