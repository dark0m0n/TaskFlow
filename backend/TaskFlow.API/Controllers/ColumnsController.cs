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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateColumn(int id, [FromBody] UpdateColumnRequest request)
    {
        var command = new UpdateColumnCommand(id, request.Title, request.Order);
        var column = await _mediator.Send(command);

        return Ok(column);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteColumn(int id)
    {
        var command = new DeleteColumnCommand(id);
        await _mediator.Send(command);

        return Ok(new { Message = "Column removed successfully" });
    }

    [HttpPost("{id:int}/cards")]
    public async Task<IActionResult> CreateCard(int id, [FromBody] CreateCardRequest request)
    {
        var command = new CreateCardCommand(id, request.Title, request.Description, request.Priority, request.DueDate);
        var card = await _mediator.Send(command);

        return Ok(card);
    }
}

public record UpdateColumnRequest(string Title, int Order);
public record CreateCardRequest(string Title, string Description, PriorityLevel Priority, DateTime? DueDate);
