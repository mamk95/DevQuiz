namespace DevQuiz.API.Dtos;

public class SessionStartedDto
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public int TotalQuestions { get; set; }

}