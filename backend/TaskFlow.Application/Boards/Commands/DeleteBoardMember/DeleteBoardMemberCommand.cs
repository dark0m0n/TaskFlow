using MediatR;

namespace TaskFlow.Application.Boards.Commands.DeleteBoardMember;

public record DeleteBoardMemberCommand(int BoardId, string MemberId) : IRequest<bool>;
