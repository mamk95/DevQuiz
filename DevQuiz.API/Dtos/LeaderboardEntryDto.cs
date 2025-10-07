namespace DevQuiz.API.Dtos;

public class LeaderboardEntryDto
{
    public required string Name { get; set; }

    public int TotalMs { get; set; }
    
    public required string AvatarUrl { get; set; }
}