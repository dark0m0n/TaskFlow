using FluentValidation;

namespace TaskFlow.Application.Boards.Commands.UpdateBoardMemberRole;

public class UpdateBoardMemberRoleCommandValidator : AbstractValidator<UpdateBoardMemberRoleCommand>
{
    public UpdateBoardMemberRoleCommandValidator()
    {
        RuleFor(v => v.BoardId)
            .GreaterThan(0).WithMessage("Incorrect BoardId");
            
        RuleFor(v => v.MemberId)
            .NotEmpty().WithMessage("MemberId required");
    }
}
