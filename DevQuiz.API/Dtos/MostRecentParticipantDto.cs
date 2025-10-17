namespace DevQuiz.API.Dtos;

public class MostRecentParticipantDto
{
    public required string Name { get; set; }

    public int TotalMs { get; set; }
    
    public required string AvatarUrl { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? Position { get; set; }

}