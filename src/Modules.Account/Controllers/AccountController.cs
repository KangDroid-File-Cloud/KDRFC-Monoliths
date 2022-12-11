using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Account.Core.Commands;
using Shared.Models.Responses;

namespace Modules.Account.Controllers;

[ApiController]
[Route("/api/account")]
[Produces("application/json")]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    ///     Join(Register) to KDRFC Service.
    /// </summary>
    /// <param name="command">Join Account Request(Register Request Body)</param>
    /// <response code="204">When succeed to register.</response>
    /// <response code="409">When user already registered to our service.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
    [HttpPost("join")]
    public async Task<IActionResult> JoinAccount(RegisterAccountCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}