namespace DevQuiz.API.Dtos;
using System.ComponentModel.DataAnnotations;

public class SubmitAnswerDto
{
    [Required]
    [StringLength(256)]
    public required string AnswerText { get; set; }
}