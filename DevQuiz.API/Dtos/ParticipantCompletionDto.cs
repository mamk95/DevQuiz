namespace DevQuiz.API.Dtos;

public class ParticipantCompletionDto
{
    public required string SessionId { get; set; }
    public required string Name { get; set; }
    public required string AvatarUrl { get; set; }
    public required string Difficulty { get; set; }
    public int TotalMs { get; set; }
    public int Ranking { get; set; }
    public bool IsTopThree { get; set; }
    public bool IsOnLeaderboard { get; set; }
}
