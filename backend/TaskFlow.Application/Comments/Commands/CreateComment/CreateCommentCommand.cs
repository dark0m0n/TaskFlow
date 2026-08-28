using MediatR;
using TaskFlow.Application.Comments.DTOs;

namespace TaskFlow.Application.Comments.Commands.CreateComment;

public record CreateCommentCommand(string Content, int CardId) : IRequest<CommentDto>;
