namespace DevQuiz.API.Dtos;

public class LeaderboardEntryDto
{
    public required string Name { get; set; }

    public int TotalMs { get; set; }
}