
namespace DevQuiz.API.Entities;
using System.ComponentModel.DataAnnotations;

public class Quiz
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string? Name { get; set; }
    [Required]
    public string? Difficulty { get; set; }

    public ICollection<QuizQuestion>? QuizQuestions { get; set; }
}
