using MediatR;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Boards.Commands.AddBoardMember;

public record AddBoardMemberCommand(int BoardId, string MemberEmail, BoardRole Role) : IRequest<bool>;
