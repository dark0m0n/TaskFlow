using FluentValidation;

namespace TaskFlow.Application.Cards.Commands.MoveCard;

public class MoveCardCommandValidator : AbstractValidator<MoveCardCommand>
{
    public MoveCardCommandValidator()
    {
        RuleFor(x => x.CardId)
            .GreaterThan(0).WithMessage("CardId must be greater than 0");

        RuleFor(x => x.TargetColumnId)
            .GreaterThan(0).WithMessage("TargetColumnId must be greater than 0");

        RuleFor(x => x.NewOrder)
            .GreaterThanOrEqualTo(1).WithMessage("NewOrder must be at least 1");
    }
}
