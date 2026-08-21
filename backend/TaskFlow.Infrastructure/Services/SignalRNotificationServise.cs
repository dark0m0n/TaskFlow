using Microsoft.AspNetCore.SignalR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Infrastructure.Hubs;

namespace TaskFlow.Infrastructure.Services;

public class SignalRNotificationServise(IHubContext<BoardHub> hubContext)
    : ISignalRNotificationService
{
    private readonly IHubContext<BoardHub> _hubContext = hubContext;

    public async Task NotifyCardMovedAsync(int boardId, int cardId, int newColumnId, int newOrder)
    {
        // Send event notifications only to users in this board's group
        await _hubContext.Clients
            .Group($"board_{boardId}")
            .SendAsync("CardMoved", new { cardId, newColumnId, newOrder });
    }

    public async Task NotifyCommentAddedAsync(int boardId, int cardId, string authorName, string content)
    {
        await _hubContext.Clients
            .Group($"board_{boardId}")
            .SendAsync("CommentAdded", new { cardId, authorName, content });
    }
}
