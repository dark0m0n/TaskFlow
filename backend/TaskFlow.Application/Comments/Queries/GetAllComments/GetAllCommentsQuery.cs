using MediatR;
using TaskFlow.Application.Comments.DTOs;

namespace TaskFlow.Application.Comments.Queries.GetAllComments;

public record GetAllCommentsQuery(int CardId) : IRequest<List<CommentDto>>;
