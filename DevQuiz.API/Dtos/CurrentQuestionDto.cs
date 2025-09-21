namespace DevQuiz.API.Dtos;

public class CurrentQuestionDto
{
    public bool Done { get; set; }

    public int? TotalMs { get; set; }

    public int? QuestionIndex { get; set; }

    public string? Type { get; set; }

    public string? Prompt { get; set; }

    public List<string>? Choices { get; set; }

    public string? InitialCode { get; set; }

    public string? TestCode { get; set; }
}