namespace DevQuiz.API.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Score
{
    [Key]
    public Guid SessionId { get; set; }

    [Required]
    public int TotalMs { get; set; }

    [ForeignKey(nameof(SessionId))]
    public Session? Session { get; set; }
}