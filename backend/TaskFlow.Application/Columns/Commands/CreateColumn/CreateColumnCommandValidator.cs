using FluentValidation;

namespace TaskFlow.Application.Columns.Commands.CreateColumn;

public class CreateColumnCommandValidator : AbstractValidator<CreateColumnCommand>
{
    public CreateColumnCommandValidator()
    {
        RuleFor(v => v.BoardId)
            .GreaterThan(0).WithMessage("Incorrect BoardId");
        
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Colunm title required")
            .MaximumLength(100).WithMessage("Title cannot be over 100 charecters");
    }
}
