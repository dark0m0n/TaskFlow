using MediatR;

namespace TaskFlow.Application.Columns.Commands.DeleteColumn;

public record DeleteColumnCommand(int Id) : IRequest<bool>;
