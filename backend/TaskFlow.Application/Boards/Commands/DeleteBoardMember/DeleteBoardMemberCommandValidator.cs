using FluentValidation;

namespace TaskFlow.Application.Boards.Commands.DeleteBoardMember;

public class DeleteBoardMemberCommandValidator : AbstractValidator<DeleteBoardMemberCommand>
{
    public DeleteBoardMemberCommandValidator()
    {
        RuleFor(v => v.BoardId)
            .GreaterThan(0).WithMessage("Incorrect BoardId");
            
        RuleFor(v => v.MemberId)
            .NotEmpty().WithMessage("MemberId required");
    }
}
