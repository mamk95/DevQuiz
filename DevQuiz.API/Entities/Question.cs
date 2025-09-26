namespace DevQuiz.API.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Question
{
    [Key]
    public int Id { get; set; }


    [Required]
    public QuestionType Type { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public required string Prompt { get; set; }

    [Required]
    [StringLength(512)]
    public required string CorrectAnswer { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? ChoicesJson { get; set; }

    public ICollection<Progress> Progresses { get; set; } = [];

    public ICollection<QuizQuestion>? QuizQuestions { get; set; }
}