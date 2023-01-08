using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Responses;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Filters;
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

    /// <summary>
    ///     Login to KDRFC Service.
    /// </summary>
    /// <param name="command">Login Request Command(Login Request Body)</param>
    /// <returns></returns>
    /// <response code="200">When user successfully logged-in.</response>
    /// <response code="401">When user's credential information is not correct.</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccessTokenResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<AccessTokenResponse>> LoginAccount(LoginCommand command)
    {
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    ///     Dropout(Remove) account
    /// </summary>
    /// <returns></returns>
    /// <response code="204">When successfully dropped out user's account.</response>
    /// <response code="401">When authorization failed.</response>
    [HttpDelete("dropout")]
    [KDRFCAuthorization]
    public async Task<IActionResult> DropoutAccount()
    {
        var contextAccount = HttpContext.GetContextAccount()!;

        // Send to Mediator
        await _mediator.Send(new DropoutUserByIdCommand
        {
            UserId = contextAccount.AccountId
        });
        return NoContent();
    }
}