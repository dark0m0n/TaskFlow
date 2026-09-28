using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Cards.Commands.DeleteCard;

public class DeleteCardHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IBoardAuthorizationService authorizationService,
    IActivityLogger activityLogger
) : IRequestHandler<DeleteCardCommand, bool>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IBoardAuthorizationService _authorizationService = authorizationService;
    private readonly IActivityLogger _activityLogger = activityLogger;

    public async Task<bool> Handle(DeleteCardCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");
        
        var card = await _context.Cards
            .Include(c => c.Column)
            .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken)
            ?? throw new KeyNotFoundException("Card not found");

        var hasAccess = await _authorizationService.HasAccessAsync(card.Column.BoardId, currentUserId, BoardRole.Member);
        if (!hasAccess)
            throw new UnauthorizedAccessException("Only board members can remove cards");

        var boardId = card.Column.BoardId;
        var cardTitle = card.Title;

        _context.Cards.Remove(card);
        await _context.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync(boardId, "CardDeleted", $"Deleted card '{cardTitle}'", cancellationToken);

        return true;
    }
}
