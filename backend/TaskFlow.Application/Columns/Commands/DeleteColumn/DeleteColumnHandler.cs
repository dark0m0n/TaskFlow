using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Columns.Commands.DeleteColumn;

public class DeleteColumnHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IBoardAuthorizationService authorizationService
) : IRequestHandler<DeleteColumnCommand, bool>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IBoardAuthorizationService _authorizationService = authorizationService;

    public async Task<bool> Handle(DeleteColumnCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId 
            ?? throw new UnauthorizedAccessException("User is not authenticated");
        
        var colunm = await _context.Columns
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Column does not exist");

        var hasAccess = await _authorizationService.HasAccessAsync(colunm.BoardId, currentUserId, BoardRole.Admin);
        if (!hasAccess)
            throw new UnauthorizedAccessException("Only Admin can remove columns");

        _context.Columns.Remove(colunm);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
