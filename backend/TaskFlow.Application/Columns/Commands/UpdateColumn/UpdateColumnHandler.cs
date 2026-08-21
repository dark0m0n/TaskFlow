using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Cards.DTOs;
using TaskFlow.Application.Columns.DTOs;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Columns.Commands.UpdateColumn;

public class UpdateColumnHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IBoardAuthorizationService authorizationService
) : IRequestHandler<UpdateColumnCommand, ColumnDto>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IBoardAuthorizationService _authorizationService = authorizationService;

    public async Task<ColumnDto> Handle(UpdateColumnCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        var column = await _context.Columns
            .FirstOrDefaultAsync(c => c.Id == request.ColumnId, cancellationToken)
            ?? throw new KeyNotFoundException("Column does not exist");
        
        var hasAccess = await _authorizationService.HasAccessAsync(column.BoardId, currentUserId, BoardRole.Member);
        if (!hasAccess)
            throw new UnauthorizedAccessException("Only Admin and Member can update columns");

        column.Title = request.Title;
        column.Order = request.Order;

        _context.Columns.Update(column);
        await _context.SaveChangesAsync(cancellationToken);

        return new ColumnDto
        {
            Id = column.Id,
            Title = column.Title,
            Order = column.Order,
            Cards = [.. column.Cards.Select(c => new CardDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Priority = c.Priority,
                DueDate = c.DueDate,
                Order = c.Order,
                AssigneeId = c.AssigneeId
            })]
        };
    }
}
