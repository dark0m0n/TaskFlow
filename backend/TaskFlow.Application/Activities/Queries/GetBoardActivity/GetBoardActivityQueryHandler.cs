using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Activities.DTOs;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Activities.Queries.GetBoardActivity;

public class GetBoardActivityQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IBoardAuthorizationService authorizationService
) : IRequestHandler<GetBoardActivityQuery, List<ActivityLogDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IBoardAuthorizationService _authorizationService = authorizationService;

    public async Task<List<ActivityLogDto>> Handle(GetBoardActivityQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        // Viewer або вище може переглядати історію активностей дошки
        var hasAccess = await _authorizationService.HasAccessAsync(request.BoardId, currentUserId, BoardRole.Viewer);
        if (!hasAccess)
            throw new UnauthorizedAccessException("User does not have access to this board");

        var limit = request.Limit > 100 ? 100 : (request.Limit <= 0 ? 20 : request.Limit);

        return await _context.ActivityLogs
            .AsNoTracking()
            .Where(a => a.BoardId == request.BoardId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .Select(a => new ActivityLogDto
            {
                Id = a.Id,
                BoardId = a.BoardId,
                UserId = a.UserId,
                Action = a.Action,
                Details = a.Details,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
