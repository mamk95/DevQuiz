namespace DevQuiz.API.Dtos;

public class AdminContactDto
{
    public required Guid ParticipantId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
}
