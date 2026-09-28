namespace TaskFlow.Application.Common.Interfaces;

public interface IActivityLogger
{
    Task LogAsync(int boardId, string action, string details, CancellationToken cancellationToken = default);
}
