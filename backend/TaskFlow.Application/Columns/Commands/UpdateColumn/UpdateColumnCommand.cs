using MediatR;
using TaskFlow.Application.Columns.DTOs;

namespace TaskFlow.Application.Columns.Commands.UpdateColumn;

public record UpdateColumnCommand(int ColumnId, string Title, int Order) : IRequest<ColumnDto>;
