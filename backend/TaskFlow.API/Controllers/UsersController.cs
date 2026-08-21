using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Users.Queries.SearchUserByEmail;

namespace TaskFlow.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("search")]
    public async Task<IActionResult> SearchByEmail([FromQuery] string email)
    {
        var query = new SearchUserByEmailQuery(email);
        var users = await _mediator.Send(query);

        return Ok(users);
    }
}
