using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Services;

public class ActivityLogger(
    IApplicationDbContext context,
    ICurrentUserService currentUserService
) : IActivityLogger
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task LogAsync(int boardId, string action, string details, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? "System";

        var log = new ActivityLog
        {
            BoardId = boardId,
            UserId = userId,
            Action = action,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
