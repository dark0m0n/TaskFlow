using FluentValidation;

namespace TaskFlow.Application.Boards.Commands.AddBoardMember;

class AddBoardMemberCommandValidator : AbstractValidator<AddBoardMemberCommand>
{
    public AddBoardMemberCommandValidator()
    {
        RuleFor(v => v.BoardId)
            .GreaterThan(0).WithMessage("Incorrect BoardId");
        
        RuleFor(v => v.MemberEmail)
            .NotEmpty().WithMessage("Member's email required")
            .EmailAddress().WithMessage("Incorrect email format");
    }
}
