namespace DevQuiz.API.Dtos;

public class LeaderboardMyScoreDto
{
    public required string Name { get; set; }

    public int TotalMs { get; set; }
    public int Position { get; set; }

    public int TotalParticipants { get; set; }

    public DateTime CompletedAtUtc { get; set; }


}