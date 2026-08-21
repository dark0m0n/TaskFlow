using Microsoft.AspNetCore.SignalR;

namespace TaskFlow.Infrastructure.Hubs;

public class BoardHub : Hub
{
    // The client calls this method when opening the board in a browser
    public async Task JoinBoard(int boardId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"board_{boardId}");
    }

    // The client calls this method when closing the board
    public async Task LeaveBoard(int boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board_{boardId}");
    }
}
