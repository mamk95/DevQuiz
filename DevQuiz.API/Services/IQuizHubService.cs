namespace DevQuiz.API.Services;

using DevQuiz.API.Dtos;

public interface IQuizHubService
{
    Task SendLeaderboardUpdateAsync(string difficulty, List<LeaderboardEntryDto> entries);
    Task SendParticipantStartedAsync(OngoingParticipantDto participant);
    Task SendParticipantProgressAsync(OngoingParticipantDto participant);
    Task SendParticipantCompletedAsync(ParticipantCompletionDto completion);
}
