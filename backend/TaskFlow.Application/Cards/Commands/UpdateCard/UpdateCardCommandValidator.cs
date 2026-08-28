using FluentValidation;

namespace TaskFlow.Application.Cards.Commands.UpdateCard;

public class UpdateCardCommandValidator : AbstractValidator<UpdateCardCommand>
{
    public UpdateCardCommandValidator()
    {
        RuleFor(v => v.CardId)
            .NotEmpty().WithMessage("CardId required");
        
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Card title required")
            .MaximumLength(100).WithMessage("Title cannot be over 100 charecters");
    }
}
