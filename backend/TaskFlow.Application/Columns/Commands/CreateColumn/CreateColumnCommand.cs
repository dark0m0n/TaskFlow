using MediatR;
using TaskFlow.Application.Columns.DTOs;

namespace TaskFlow.Application.Columns.Commands.CreateColumn;

public record CreateColumnCommand(int BoardId, string Title) : IRequest<ColumnDto>;
