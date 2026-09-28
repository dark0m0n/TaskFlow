using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Activities.Queries.GetBoardActivity;
using TaskFlow.Application.Boards.Commands.AddBoardMember;
using TaskFlow.Application.Boards.Commands.CreateBoard;
using TaskFlow.Application.Boards.Commands.DeleteBoardMember;
using TaskFlow.Application.Boards.Commands.UpdateBoardMemberRole;
using TaskFlow.Application.Boards.Queries.GetBoardById;
using TaskFlow.Application.Boards.Queries.GetUserBoards;
using TaskFlow.Application.Columns.Commands.CreateColumn;
using TaskFlow.Domain.Entities;

namespace TaskFlow.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BoardsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBoardRequest request)
    {
        var command = new CreateBoardCommand(request.Title, request.Description);
        var boardId = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetById), new { boardId = boardId }, new { Id = boardId });
    }

    [HttpGet("{boardId:int}")]
    public async Task<IActionResult> GetById(int boardId)
    {
        var query = new GetBoardByIdQuery(boardId);
        var board = await _mediator.Send(query);

        if (board == null)
            return NotFound("Board not found");

        return Ok(board);
    }

    [HttpPost("{boardId:int}/members")]
    public async Task<IActionResult> AddMember(int boardId, [FromBody] AddMemberRequest request)
    {
        var command = new AddBoardMemberCommand(boardId, request.Email, request.Role);
        await _mediator.Send(command);

        return Ok(new { Message = "Member added successfully" });
    }

    [HttpDelete("{boardId:int}/members/{userId}")]
    public async Task<IActionResult> DeleteMember(int boardId, string userId)
    {
        var command = new DeleteBoardMemberCommand(boardId, userId);
        await _mediator.Send(command);

        return Ok(new { Message = "Member deleted successfully" });
    }

    [HttpPut("{boardId:int}/members/{userId}/role")]
    public async Task<IActionResult> UpdateMember(int boardId, string userId, [FromBody] UpdateMemberRequest request)
    {
        var command = new UpdateBoardMemberRoleCommand(boardId, userId, request.Role);
        await _mediator.Send(command);

        return Ok(new { Message = "Member role updated successfully" });
    }

    [HttpPost("{boardId:int}/columns")]
    public async Task<IActionResult> CreateColumn(int boardId, [FromBody] CreateColumnRequest request)
    {
        var command = new CreateColumnCommand(boardId, request.Title);
        var column = await _mediator.Send(command);

        return Ok(column);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserBoards()
    {
        var query = new GetUserBoardsQuery();
        var boards = await _mediator.Send(query);

        return Ok(boards);
    }

    [HttpGet("{boardId:int}/activity")]
    public async Task<IActionResult> GetActivity(int boardId, [FromQuery] int limit = 20)
    {
        var query = new GetBoardActivityQuery(boardId, limit);
        var activities = await _mediator.Send(query);

        return Ok(activities);
    }
}

public record CreateBoardRequest(string Title, string Description);
public record AddMemberRequest(string Email, BoardRole Role);
public record UpdateMemberRequest(BoardRole Role);
public record CreateColumnRequest(string Title);
