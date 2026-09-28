using MediatR;
using TaskFlow.Application.Activities.DTOs;

namespace TaskFlow.Application.Activities.Queries.GetBoardActivity;

public record GetBoardActivityQuery(int BoardId, int Limit = 20) : IRequest<List<ActivityLogDto>>;
