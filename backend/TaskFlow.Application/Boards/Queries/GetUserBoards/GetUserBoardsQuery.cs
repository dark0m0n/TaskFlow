using MediatR;
using TaskFlow.Application.Boards.DTOs;

namespace TaskFlow.Application.Boards.Queries.GetUserBoards;

public record GetUserBoardsQuery() : IRequest<List<BoardDto>>;
