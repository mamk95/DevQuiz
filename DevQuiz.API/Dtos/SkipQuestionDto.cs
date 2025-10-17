namespace DevQuiz.API.Dtos;

public class SkipResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int PenaltyMs { get; set; }
    public bool QuizCompleted { get; set; }
    public int? TotalMs { get; set; }
}