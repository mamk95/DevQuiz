namespace DevQuiz.API.Dtos;
using System.ComponentModel.DataAnnotations;

public class SubmitEmailDto
{
    [Required]
    [EmailAddress]
    [StringLength(128)]
    public required string Email { get; set; }
}