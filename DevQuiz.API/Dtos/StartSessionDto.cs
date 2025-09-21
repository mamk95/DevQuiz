namespace DevQuiz.API.Dtos;
using System.ComponentModel.DataAnnotations;

public class StartSessionDto
{
    [Required]
    [StringLength(64, MinimumLength = 1)]
    public required string Name { get; set; }

    [Required]
    [RegularExpression(@"^\+47\d{8}$", ErrorMessage = "Phone must be in format +47XXXXXXXX")]
    public required string Phone { get; set; }
}