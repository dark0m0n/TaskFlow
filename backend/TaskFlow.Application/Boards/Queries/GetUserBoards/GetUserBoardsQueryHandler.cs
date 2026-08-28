using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Boards.DTOs;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Boards.Queries.GetUserBoards;

public class GetUsersBoardsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService
) : IRequestHandler<GetUserBoardsQuery, List<BoardDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<List<BoardDto>> Handle(GetUserBoardsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        return await _context.Boards
            .AsNoTracking()
            .Where(b => b.Members.Any(m => m.UserId == currentUserId))
            .Select(b => new BoardDto
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                OwnerId = b.OwnerId,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
