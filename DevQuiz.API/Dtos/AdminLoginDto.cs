namespace DevQuiz.API.Dtos;
using System.ComponentModel.DataAnnotations;

public class AdminLoginDto
{
    [Required]
    public required string Password { get; set; }
}
