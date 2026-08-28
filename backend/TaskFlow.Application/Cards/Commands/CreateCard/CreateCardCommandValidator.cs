using FluentValidation;

namespace TaskFlow.Application.Cards.Commands.CreateCard;

public class CreateCardCommandValidator : AbstractValidator<CreateCardCommand>
{
    public CreateCardCommandValidator()
    {
        RuleFor(v => v.ColumnId)
            .NotEmpty().WithMessage("ColunmId required");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Card title required")
            .MaximumLength(100).WithMessage("Title cannot be over 100 charecters");
    }
}
