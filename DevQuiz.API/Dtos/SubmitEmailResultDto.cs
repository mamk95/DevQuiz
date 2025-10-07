namespace DevQuiz.API.Dtos;
using System.ComponentModel.DataAnnotations;

public class SubmitEmailResultDto
{
    [Required]
    public bool Success { get; set; }

    [Required]
    public required string Message { get; set; }
}