using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Comments.DTOs;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Comments.Queries.GetAllComments;

public class GetAllCommentsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllCommentsQuery, List<CommentDto>>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<List<CommentDto>> Handle(GetAllCommentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Comments
        .AsNoTracking()
        .Where(c => c.CardId == request.CardId)
        .OrderBy(c => c.CreatedAt)
        .Select(c => new CommentDto
        {
            Id = c.Id,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            UserId = c.UserId,
            CardId = c.CardId
        })
        .ToListAsync(cancellationToken);
    }
}
