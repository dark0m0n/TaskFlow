using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Cards.DTOs;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Cards.Commands.UpdateCard;

public class UpdateCardHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IBoardAuthorizationService authorizationService
) : IRequestHandler<UpdateCardCommand, CardDto>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IBoardAuthorizationService _authorizationService = authorizationService;

    public async Task<CardDto> Handle(UpdateCardCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        var card = await _context.Cards
            .Include(c => c.Column)
            .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken)
            ?? throw new KeyNotFoundException("Card not found");

        var hasAccess = await _authorizationService.HasAccessAsync(card.Column.BoardId, currentUserId, BoardRole.Member);
        if (!hasAccess)
            throw new UnauthorizedAccessException("Only board members can edit cards");

        if (!string.IsNullOrWhiteSpace(request.AssigneeId))
        {
            var isAssigneeMember = await _context.BoardMembers
                .AnyAsync(m => m.BoardId == card.Column.BoardId && m.UserId == request.AssigneeId, cancellationToken);
            if (!isAssigneeMember)
                throw new InvalidOperationException("Assignee must be a member of this board");
        }

        card.Title = request.Title;
        card.Description = request.Description;
        card.Priority = request.Priority;
        card.DueDate = request.DueDate;
        card.AssigneeId = request.AssigneeId;

        _context.Cards.Update(card);
        await _context.SaveChangesAsync(cancellationToken);

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
