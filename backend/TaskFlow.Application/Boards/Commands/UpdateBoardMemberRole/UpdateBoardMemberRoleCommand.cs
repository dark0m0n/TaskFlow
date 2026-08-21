using MediatR;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Boards.Commands.UpdateBoardMemberRole;

public record UpdateBoardMemberRoleCommand(int BoardId, string MemberId, BoardRole Role) : IRequest<bool>;
