using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Cards.DTOs;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Cards.Commands.CreateCard;

public class CreateCardHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IBoardAuthorizationService authorizationService
) : IRequestHandler<CreateCardCommand, CardDto>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IBoardAuthorizationService _authorizationService = authorizationService;

    public async Task<CardDto> Handle(CreateCardCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        var column = await _context.Columns
            .FirstOrDefaultAsync(c => c.Id == request.ColunmId, cancellationToken)
            ?? throw new KeyNotFoundException("Column does not exist");
        
        var hasAccess = await _authorizationService.HasAccessAsync(column.BoardId, currentUserId, BoardRole.Member);
        if (!hasAccess)
            throw new UnauthorizedAccessException("Only Member and Admin can create cards");
        
        var maxOrder = await _context.Cards
            .Where(c => c.ColumnId == request.ColunmId)
            .MaxAsync(c => (int?)c.Order, cancellationToken) ?? 0;

        var card = new Card
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            Order = maxOrder + 1,
            ColumnId = request.ColunmId,
            AssigneeId = currentUserId
        };

        _context.Cards.Add(card);
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
