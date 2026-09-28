using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Comments.DTOs;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Comments.Commands.CreateComment;

public class CreateCommentHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IBoardAuthorizationService authorizationService,
    IActivityLogger activityLogger
) : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IBoardAuthorizationService _authorizationService = authorizationService;
    private readonly IActivityLogger _activityLogger = activityLogger;

    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        var card = await _context.Cards
            .Include(c => c.Column)
            .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken)
            ?? throw new KeyNotFoundException("Card not found");

        var hasAccess = await _authorizationService.HasAccessAsync(card.Column.BoardId, currentUserId, BoardRole.Member);
        if (!hasAccess)
            throw new UnauthorizedAccessException("Only board members can add comments");

        var comment = new Comment
        {
            Content = request.Content,
            UserId = currentUserId,
            CardId = request.CardId
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync(card.Column.BoardId, "CommentAdded", $"Added comment to card '{card.Title}'", cancellationToken);

        return new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UserId = comment.UserId,
            CardId = comment.CardId
        };
    }
}
