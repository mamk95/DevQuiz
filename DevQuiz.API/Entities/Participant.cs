namespace DevQuiz.API.Entities;
using System.ComponentModel.DataAnnotations;

public class Participant
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(64)]
    public required string Name { get; set; }

    [Required]
    [StringLength(32)]
    public required string Phone { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<Session> Sessions { get; set; } = [];
}