namespace DevQuiz.API.Dtos;

public class SkipQuestionDto
{
    public required int QuestionIndex { get; set; }
    public required int PenaltyTimeMs { get; set; }
}