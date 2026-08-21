using FluentValidation;

namespace TaskFlow.Application.Columns.Commands.UpdateColumn;

public class UpdateColumnCommandValidator : AbstractValidator<UpdateColumnCommand>
{
    public UpdateColumnCommandValidator()
    {
        RuleFor(v => v.ColumnId)
            .GreaterThan(0).WithMessage("Incorrect ColumnId");
        
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Colunm title required")
            .MaximumLength(100).WithMessage("Title cannot be over 100 charecters");

        RuleFor(v => v.Order)
            .GreaterThan(0).WithMessage("Order must be greater than 0");
    }
}
