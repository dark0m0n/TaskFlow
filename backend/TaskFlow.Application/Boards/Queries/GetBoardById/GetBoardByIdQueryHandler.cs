using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Boards.DTOs;
using TaskFlow.Application.Cards.DTOs;
using TaskFlow.Application.Columns.DTOs;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Boards.Queries.GetBoardById;

public class GetBoardByIdQueryHandler(IApplicationDbContext context) 
    : IRequestHandler<GetBoardByIdQuery, BoardDto?>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<BoardDto?> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
    {
        var board = await _context.Boards
            .AsNoTracking()
            .Include(b => b.Columns.OrderBy(c => c.Order))
                .ThenInclude(c => c.Cards.OrderBy(card => card.Order))
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (board == null) return null;

        return new BoardDto
        {
            Id = board.Id,
            Title = board.Title,
            Description = board.Description,
            OwnerId = board.OwnerId,
            CreatedAt = board.CreatedAt,
            Members = [.. board.Members.Select(m => new BoardMemberDto
            {
                UserId = m.UserId,
                Role = m.Role
            })],
            Columns = [.. board.Columns.Select(c => new ColumnDto
            {
                Id = c.Id,
                Title = c.Title,
                Order = c.Order,
                Cards = [.. c.Cards.Select(card => new CardDto
                {
                    Id = card.Id,
                    Title = card.Title,
                    Description = card.Description,
                    Priority = card.Priority,
                    DueDate = card.DueDate,
                    Order = card.Order,
                    AssigneeId = card.AssigneeId
                })]
            })]
        };
    }
}