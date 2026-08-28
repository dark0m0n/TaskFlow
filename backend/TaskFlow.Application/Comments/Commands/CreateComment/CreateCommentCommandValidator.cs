using FluentValidation;

namespace TaskFlow.Application.Comments.Commands.CreateComment;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(v => v.Content)
            .NotEmpty().WithMessage("Content required")
            .MaximumLength(500).WithMessage("Comment content cannot be over 500 charecters");

        RuleFor(v => v.CardId)
            .NotEmpty().WithMessage("CardId required");
    }
}
