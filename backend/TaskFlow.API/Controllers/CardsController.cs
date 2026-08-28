using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Cards.Commands.DeleteCard;
using TaskFlow.Application.Cards.Commands.UpdateCard;
using TaskFlow.Application.Comments.Commands.CreateComment;
using TaskFlow.Application.Comments.Queries.GetAllComments;
using TaskFlow.Domain.Entities;

namespace TaskFlow.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CardsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPut("{cardId:int}")]
    public async Task<IActionResult> UpdateCard(int cardId, [FromBody] UpdateCardRequest request)
    {
        var command = new UpdateCardCommand(
            cardId,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            request.AssigneeId
        );
        var card = await _mediator.Send(command);

        return Ok(card);
    }

    [HttpDelete("{cardId:int}")]
    public async Task<IActionResult> DeleteCard(int cardId)
    {
        var command = new DeleteCardCommand(cardId);
        await _mediator.Send(command);

        return Ok(new { Message = "Card removed successfully" });
    }

    [HttpPost("{cardId:int}/comments")]
    public async Task<IActionResult> CreateComment(int cardId, [FromBody] CreateCommentRequest request)
    {
        var command = new CreateCommentCommand(request.Content, cardId);
        var comment = await _mediator.Send(command);

        return Ok(comment);
    }

    [HttpGet("{cardId:int}/comments")]
    public async Task<IActionResult> GetAllComments(int cardId)
    {
        var query = new GetAllCommentsQuery(cardId);
        var comments = await _mediator.Send(query);

        return Ok(comments);
    }
}

public record UpdateCardRequest(
    string Title, 
    string Description, 
    PriorityLevel Priority, 
    DateTime? DueDate,
    string? AssigneeId = null
);
public record CreateCommentRequest(string Content);
