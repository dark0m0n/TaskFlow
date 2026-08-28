using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Cards.Commands.CreateCard;
using TaskFlow.Application.Columns.Commands.DeleteColumn;
using TaskFlow.Application.Columns.Commands.UpdateColumn;
using TaskFlow.Domain.Entities;

namespace TaskFlow.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ColumnsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPut("{columnId:int}")]
    public async Task<IActionResult> UpdateColumn(int columnId, [FromBody] UpdateColumnRequest request)
    {
        var command = new UpdateColumnCommand(columnId, request.Title, request.Order);
        var column = await _mediator.Send(command);

        return Ok(column);
    }

    [HttpDelete("{columnId:int}")]
    public async Task<IActionResult> DeleteColumn(int columnId)
    {
        var command = new DeleteColumnCommand(columnId);
        await _mediator.Send(command);

        return Ok(new { Message = "Column removed successfully" });
    }

    [HttpPost("{columnId:int}/cards")]
    public async Task<IActionResult> CreateCard(int columnId, [FromBody] CreateCardRequest request)
    {
        var command = new CreateCardCommand(
            columnId,
            request.Title, 
            request.Description, 
            request.Priority, 
            request.DueDate, 
            request.AssigneeId
        );
        var card = await _mediator.Send(command);

        return Ok(card);
    }
}

public record UpdateColumnRequest(string Title, int Order);
public record CreateCardRequest(
    string Title,
    string Description,
    PriorityLevel Priority,
    DateTime? DueDate,
    string? AssigneeId = null
);
