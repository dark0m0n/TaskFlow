using FluentValidation;

namespace TaskFlow.Application.Columns.Commands.DeleteColumn;

public class DeleteColumnCommandValidator : AbstractValidator<DeleteColumnCommand>
{
    public DeleteColumnCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ColunmId required");
    }
}
