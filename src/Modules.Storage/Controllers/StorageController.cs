using MediatR;
using Microsoft.AspNetCore.Mvc;
using Modules.Storage.Core.Commands;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Filters;

namespace Modules.Storage.Controllers;

[ApiController]
[Route("/api/storage")]
[Produces("application/json")]
public class StorageController : ControllerBase
{
    private readonly IMediator _mediator;

    public StorageController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("list")]
    [KDRFCAuthorization]
    public async Task<IActionResult> ListFolderAsync(string folderId)
    {
        var userId = HttpContext.GetUserId()!;

        return Ok(await _mediator.Send(new ListStorageByFolderIdCommand
        {
            AccountId = userId,
            FolderId = folderId
        }));
    }
}