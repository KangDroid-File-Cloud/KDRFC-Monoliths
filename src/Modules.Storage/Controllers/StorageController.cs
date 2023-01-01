using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Storage.Core.Commands;
using Modules.Storage.Core.Models.Requests;
using Modules.Storage.Core.Models.Responses;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Filters;
using Shared.Models.Responses;

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

    /// <summary>
    ///     Returns list of folder/file entries with given folder id.
    /// </summary>
    /// <param name="folderId">(Required) Folder ID to lookup.</param>
    /// <returns></returns>
    /// <response code="200">When successfully got folder's contents.</response>
    /// <response code="401">When user's credential information is not correct.</response>
    [HttpGet("list")]
    [KDRFCAuthorization]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BlobProjection>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> ListFolderAsync(string folderId)
    {
        var userId = HttpContext.GetUserId()!;

        return Ok(await _mediator.Send(new ListStorageByFolderIdCommand
        {
            AccountId = userId,
            FolderId = folderId
        }));
    }

    /// <summary>
    ///     Create Folder under parent folder.
    /// </summary>
    /// <param name="request">Create Blob Folder Request. See schemas below.</param>
    /// <returns></returns>
    /// <response code="200">When successfully create blob folder on parent.</response>
    /// <response code="401">When user's credential information is not correct.</response>
    [HttpPost("folders")]
    [KDRFCAuthorization]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BlobProjection))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> CreateFolderAsync(CreateBlobFolderRequest request)
    {
        var userId = HttpContext.GetUserId()!;

        return Ok(await _mediator.Send(new CreateBlobFolderCommand
        {
            AccountId = userId,
            ParentFolderId = request.ParentFolderId,
            FolderName = request.FolderName
        }));
    }
}