namespace DevQuiz.API.Dtos;

public class OngoingParticipantDto
{
    public required string SessionId { get; set; }
    public required string Name { get; set; }
    public required string AvatarUrl { get; set; }
    public required string Difficulty { get; set; }
    public long StartedAtMs { get; set; }
    public long LastActivityMs { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalPenaltyMs { get; set; }
}
