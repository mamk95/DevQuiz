namespace DevQuiz.API.Dtos;

public class ResumeSessionDto
{
    public int QuestionIndex { get; set; }
    public bool Finished { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string ParticipantPhone { get; set; } = string.Empty;
    public int TotalTimeMs { get; set; }
    public bool Success { get; set; }
}