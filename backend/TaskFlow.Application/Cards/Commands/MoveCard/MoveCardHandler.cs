using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Cards.DTOs;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Cards.Commands.MoveCard;

public class MoveCardHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IBoardAuthorizationService authorizationService,
    IActivityLogger activityLogger,
    ISignalRNotificationService signalRService
) : IRequestHandler<MoveCardCommand, CardDto>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IBoardAuthorizationService _authorizationService = authorizationService;
    private readonly IActivityLogger _activityLogger = activityLogger;
    private readonly ISignalRNotificationService _signalRService = signalRService;

    public async Task<CardDto> Handle(MoveCardCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        var card = await _context.Cards
            .Include(c => c.Column)
            .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken)
            ?? throw new KeyNotFoundException("Card not found");

        var boardId = card.Column.BoardId;

        var hasAccess = await _authorizationService.HasAccessAsync(boardId, currentUserId, BoardRole.Member);
        if (!hasAccess)
            throw new UnauthorizedAccessException("Only board members can move cards");

        string destinationColumnTitle = card.Column.Title;

        if (card.ColumnId != request.TargetColumnId)
        {
            var targetColumn = await _context.Columns
                .FirstOrDefaultAsync(c => c.Id == request.TargetColumnId, cancellationToken)
                ?? throw new KeyNotFoundException("Target column not found");

            if (targetColumn.BoardId != boardId)
                throw new InvalidOperationException("Cannot move card to a column on a different board");

            destinationColumnTitle = targetColumn.Title;
            card.ColumnId = request.TargetColumnId;
        }

        card.Order = request.NewOrder;

        await _context.SaveChangesAsync(cancellationToken);

        // Аудит події в базу даних (Activity Log)
        await _activityLogger.LogAsync(
            boardId, 
            "CardMoved", 
            $"Moved card '{card.Title}' to '{destinationColumnTitle}' (position {card.Order})", 
            cancellationToken
        );

        // Real-time сповіщення через WebSockets у групу дошки
        await _signalRService.NotifyCardMovedAsync(boardId, card.Id, request.TargetColumnId, request.NewOrder);

        return new CardDto
        {
            Id = card.Id,
            Title = card.Title,
            Description = card.Description,
            Priority = card.Priority,
            DueDate = card.DueDate,
            Order = card.Order,
            AssigneeId = card.AssigneeId
        };
    }
}
