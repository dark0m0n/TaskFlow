using FluentValidation;

namespace TaskFlow.Application.Cards.Commands.DeleteCard;

public class DeleteCardCommandValidator : AbstractValidator<DeleteCardCommand>
{
    public DeleteCardCommandValidator()
    {
        RuleFor(v => v.CardId)
            .NotEmpty().WithMessage("CardId required");
    }
}
