using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Boards.Commands.DeleteBoardMember;

public class DeleteBoardMemberHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IBoardAuthorizationService authorizationService
) : IRequestHandler<DeleteBoardMemberCommand, bool>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IBoardAuthorizationService _authorizationService = authorizationService;

    public async Task<bool> Handle(DeleteBoardMemberCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId 
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        var board = await _context.Boards
            .FirstOrDefaultAsync(b => b.Id == request.BoardId, cancellationToken)
            ?? throw new KeyNotFoundException("Board does not exist");

        if (board.OwnerId == request.MemberId)
            throw new InvalidOperationException("Cannot remove the owner of the board");

        var isSelfRemoval = currentUserId == request.MemberId;
        if (!isSelfRemoval)
        {
            var hasAccess = await _authorizationService.HasAccessAsync(request.BoardId, currentUserId, BoardRole.Admin);
            if (!hasAccess)
                throw new UnauthorizedAccessException("Only Admin can remove other members");
        }

        var targetMember = await _context.BoardMembers
            .FirstOrDefaultAsync(m => m.BoardId == request.BoardId && m.UserId == request.MemberId, cancellationToken)
            ?? throw new InvalidOperationException("This user is not board member");

        _context.BoardMembers.Remove(targetMember);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
