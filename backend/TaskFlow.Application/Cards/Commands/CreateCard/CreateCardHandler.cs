using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Cards.DTOs;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Cards.Commands.CreateCard;

public class CreateCardHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IBoardAuthorizationService authorizationService,
    IActivityLogger activityLogger
) : IRequestHandler<CreateCardCommand, CardDto>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IBoardAuthorizationService _authorizationService = authorizationService;
    private readonly IActivityLogger _activityLogger = activityLogger;

    public async Task<CardDto> Handle(CreateCardCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        var column = await _context.Columns
            .FirstOrDefaultAsync(c => c.Id == request.ColumnId, cancellationToken)
            ?? throw new KeyNotFoundException("Column not found");
        
        var hasAccess = await _authorizationService.HasAccessAsync(column.BoardId, currentUserId, BoardRole.Member);
        if (!hasAccess)
            throw new UnauthorizedAccessException("Only board members can create cards");

        if (!string.IsNullOrWhiteSpace(request.AssigneeId))
        {
            var isAssigneeMember = await _context.BoardMembers
                .AnyAsync(m => m.BoardId == column.BoardId && m.UserId == request.AssigneeId, cancellationToken);
            if (!isAssigneeMember)
                throw new InvalidOperationException("Assignee must be a member of this board");
        }
        
        var maxOrder = await _context.Cards
            .Where(c => c.ColumnId == request.ColumnId)
            .MaxAsync(c => (int?)c.Order, cancellationToken) ?? 0;

        var card = new Card
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            Order = maxOrder + 1,
            ColumnId = request.ColumnId,
            AssigneeId = request.AssigneeId
        };

        _context.Cards.Add(card);
        await _context.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync(column.BoardId, "CardCreated", $"Created card '{card.Title}'", cancellationToken);

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
