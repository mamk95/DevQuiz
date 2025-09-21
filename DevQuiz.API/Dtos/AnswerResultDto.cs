namespace DevQuiz.API.Dtos;

public class AnswerResultDto
{
    public bool Correct { get; set; }

    public int? PenaltyMsAdded { get; set; }

    public bool? QuizCompleted { get; set; }

    public int? TotalMs { get; set; }
}