using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Boards.Commands.CreateBoard;

public class CreateBoardHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    : IRequestHandler<CreateBoardCommand, int>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<int> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId 
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        var board = new Board
        {
            Title = request.Title,
            Description = request.Description,
            OwnerId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            Members =
            [
                new BoardMember
                {
                    UserId = currentUserId,
                    Role = BoardRole.Admin
                }
            ]
        };

        _context.Boards.Add(board);
        await _context.SaveChangesAsync(cancellationToken);

        return board.Id;
    }
}
