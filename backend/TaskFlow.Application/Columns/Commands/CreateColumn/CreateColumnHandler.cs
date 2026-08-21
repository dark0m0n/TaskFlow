using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Columns.DTOs;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Columns.Commands.CreateColumn;

public class CreateColumnHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IBoardAuthorizationService authorizationService
) : IRequestHandler<CreateColumnCommand, ColumnDto>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IBoardAuthorizationService _authorizationService = authorizationService;

    public async Task<ColumnDto> Handle(CreateColumnCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        var hasAccess = await _authorizationService.HasAccessAsync(request.BoardId, currentUserId, BoardRole.Member);
        if (!hasAccess)
            throw new UnauthorizedAccessException("Only Admin and Member can create columns");
        
        var maxOrder = await _context.Columns
            .Where(c => c.BoardId == request.BoardId)
            .MaxAsync(c => (int?)c.Order, cancellationToken) ?? 0;

        var column = new Column
        {
            BoardId = request.BoardId,
            Title = request.Title,
            Order = maxOrder + 1
        };

        _context.Columns.Add(column);
        await _context.SaveChangesAsync(cancellationToken);

        return new ColumnDto
        {
            Id = column.Id,
            Title = column.Title,
            Order = column.Order,
            Cards = []
        };
    }
}
