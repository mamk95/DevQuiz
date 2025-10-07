namespace DevQuiz.API.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class QuizQuestion
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int QuizId { get; set; }
    [ForeignKey(nameof(QuizId))]
    public Quiz? Quiz { get; set; }

    [Required]
    public int QuestionId { get; set; }
    [ForeignKey(nameof(QuestionId))]
    public Question? Question { get; set; }

    [Required]
    public int Sequence { get; set; }
}
