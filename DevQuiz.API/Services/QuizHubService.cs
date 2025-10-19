namespace DevQuiz.API.Services;

using DevQuiz.API.Dtos;
using DevQuiz.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

public class QuizHubService(IHubContext<QuizHub> hubContext, ILogger<QuizHubService> logger) : IQuizHubService
{
    public async Task SendLeaderboardUpdateAsync(string difficulty, List<LeaderboardEntryDto> entries)
    {
        logger.LogInformation("Sending leaderboard update for {Difficulty} with {Count} entries", difficulty, entries.Count);
        await hubContext.Clients.All.SendAsync("LeaderboardUpdate", difficulty, entries);
    }

    public async Task SendParticipantStartedAsync(OngoingParticipantDto participant)
    {
        logger.LogInformation("Sending participant started: {Name}", participant.Name);
        await hubContext.Clients.All.SendAsync("ParticipantStarted", participant);
    }

    public async Task SendParticipantProgressAsync(OngoingParticipantDto participant)
    {
        logger.LogInformation("Sending participant progress: {Name} Q{Question}", participant.Name, participant.CurrentQuestionIndex + 1);
        await hubContext.Clients.All.SendAsync("ParticipantProgress", participant);
    }

    public async Task SendParticipantCompletedAsync(ParticipantCompletionDto completion)
    {
        logger.LogInformation("Sending participant completed: {Name} Rank {Rank}", completion.Name, completion.Ranking);
        await hubContext.Clients.All.SendAsync("ParticipantCompleted", completion);
    }
}
