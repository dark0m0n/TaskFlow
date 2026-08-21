namespace TaskFlow.Application.Common.Interfaces;

public interface ISignalRNotificationService
{
    Task NotifyCardMovedAsync(int boardId, int cardId, int newColumnId, int newOrder);
    Task NotifyCommentAddedAsync(int boardId, int cardId, string authorName, string content);
}
