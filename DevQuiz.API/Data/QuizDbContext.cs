namespace DevQuiz.API.Data;
using DevQuiz.API.Entities;
using Microsoft.EntityFrameworkCore;

public class QuizDbContext(DbContextOptions<QuizDbContext> options) : DbContext(options)
{
    public DbSet<Participant> Participants { get; set; }

    public DbSet<Session> Sessions { get; set; }

    public DbSet<Question> Questions { get; set; }

    public DbSet<Progress> Progresses { get; set; }

    public DbSet<Score> Scores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasIndex(e => e.Phone).IsUnique();
            entity.Property(e => e.CreatedAtUtc).HasColumnType("datetime2");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.Property(e => e.CurrentQuestionIndex).HasDefaultValue(0);
            entity.Property(e => e.StartedAtUtc).HasColumnType("datetime2");
            entity.Property(e => e.CompletedAtUtc).HasColumnType("datetime2");
            entity.HasOne(e => e.Participant)
                .WithMany(p => p.Sessions)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasIndex(e => e.Sequence).IsUnique();
            entity.Property(e => e.Type).HasConversion<byte>();
        });

        modelBuilder.Entity<Progress>(entity =>
        {
            entity.Property(e => e.StartAtUtc).HasColumnType("datetime2");
            entity.Property(e => e.PenaltyMs).HasDefaultValue(0);
            entity.Property(e => e.IsCorrect).HasDefaultValue(false);
            entity.HasOne(e => e.Session)
                .WithMany(s => s.Progresses)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Question)
                .WithMany(q => q.Progresses)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.SessionId, e.QuestionId });
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasOne(e => e.Session)
                .WithOne(s => s.Score)
                .HasForeignKey<Score>(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.TotalMs);
        });
    }
}