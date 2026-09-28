using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Boards.Commands.AddBoardMember;

class AddBoardMemberHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IBoardAuthorizationService authorizationService,
    IActivityLogger activityLogger
) : IRequestHandler<AddBoardMemberCommand, bool>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IBoardAuthorizationService _authorizationService = authorizationService;
    private readonly IActivityLogger _activityLogger = activityLogger;

    public async Task<bool> Handle(AddBoardMemberCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId 
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        var hasAccess = await _authorizationService.HasAccessAsync(request.BoardId, currentUserId, BoardRole.Admin);
        if (!hasAccess)
            throw new UnauthorizedAccessException("Only Admin can add new members");

        var targetUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.MemberEmail, cancellationToken)
            ?? throw new KeyNotFoundException("User with such email not found");
        
        var existingMember = await _context.BoardMembers
            .AnyAsync(m => m.BoardId == request.BoardId && m.UserId == targetUser.Id, cancellationToken);

        if (existingMember)
            throw new InvalidOperationException("This user already is a board member");
        
        var member = new BoardMember
        {
            BoardId = request.BoardId,
            UserId = targetUser.Id,
            Role = request.Role
        };

        _context.BoardMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync(request.BoardId, "MemberAdded", $"Added member '{targetUser.Email}' as {request.Role}", cancellationToken);

        return true;
    }
}
