using MediatR;
using TaskFlow.Application.Boards.DTOs;

namespace TaskFlow.Application.Boards.Queries.GetBoardById;

public record GetBoardByIdQuery(int Id) : IRequest<BoardDto?>;
