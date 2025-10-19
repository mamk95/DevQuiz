namespace DevQuiz.API.Dtos;

public class AdminAuthResultDto
{
    public required bool Success { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
}
