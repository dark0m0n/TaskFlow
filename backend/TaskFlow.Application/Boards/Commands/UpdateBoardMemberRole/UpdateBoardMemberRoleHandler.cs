using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Boards.Commands.UpdateBoardMemberRole;

public class UpdateBoardMemberRoleHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IBoardAuthorizationService authorizationService
) : IRequestHandler<UpdateBoardMemberRoleCommand, bool>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IBoardAuthorizationService _authorizationService = authorizationService;

    public async Task<bool> Handle(UpdateBoardMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        var hasAccess = await _authorizationService.HasAccessAsync(request.BoardId, currentUserId, BoardRole.Admin);
        if (!hasAccess)
            throw new UnauthorizedAccessException("Only Admin can change roles");
        
        if (currentUserId == request.MemberId)
            throw new InvalidOperationException("User cannot change its own role");

        var board = await _context.Boards
            .FirstOrDefaultAsync(b => b.Id == request.BoardId, cancellationToken)
            ?? throw new KeyNotFoundException("Board does not exists");
        
        if (board.OwnerId == request.MemberId)
            throw new InvalidOperationException("Cannot change role of the owner of the board");

        var targetMember = await _context.BoardMembers
            .FirstOrDefaultAsync(m => m.BoardId == request.BoardId && m.UserId == request.MemberId, cancellationToken)
            ?? throw new InvalidOperationException("This user is not board member");

        targetMember.Role = request.Role;

        _context.BoardMembers.Update(targetMember);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
