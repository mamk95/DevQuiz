namespace DevQuiz.API.Dtos;

public class AdminLeaderboardEntryDto
{
    public required Guid ParticipantId { get; set; }
    public required string Name { get; set; }
    public required string Phone { get; set; }
    public required int TotalMs { get; set; }
    public required string AvatarUrl { get; set; }
    public required string Difficulty { get; set; }
}
