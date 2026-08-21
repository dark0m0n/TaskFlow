using FluentValidation;

namespace TaskFlow.Application.Boards.Commands.CreateBoard;

public class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Board title required")
            .MaximumLength(100).WithMessage("Title cannot be over 100 charecters");
    }
}
