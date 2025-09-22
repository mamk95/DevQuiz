namespace DevQuiz.API.Dtos;
using System.ComponentModel.DataAnnotations;

public class SubmitAnswerDto
{
    [Required]
    [StringLength(512)]
    public required string AnswerText { get; set; }
}