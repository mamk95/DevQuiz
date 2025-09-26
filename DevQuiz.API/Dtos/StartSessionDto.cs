namespace DevQuiz.API.Dtos;
using System.ComponentModel.DataAnnotations;

public class StartSessionDto
{
    [Required]
    [StringLength(64, MinimumLength = 1)]
    public required string Name { get; set; }

    [Required]
    [RegularExpression(@"^\+\d{1,5}\d{4,15}$", ErrorMessage = "Phone must be a valid international number (e.g., +1234567890)")]
    public required string Phone { get; set; }

    [Required]
    [StringLength(256, MinimumLength = 1)]
    public required string AvatarUrl { get; set; }
}