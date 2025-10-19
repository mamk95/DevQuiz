namespace DevQuiz.API.Hubs;

using Microsoft.AspNetCore.SignalR;

public class QuizHub : Hub
{
    // Clients connect and disconnect automatically
    // Server-side methods broadcast updates to all connected clients

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
