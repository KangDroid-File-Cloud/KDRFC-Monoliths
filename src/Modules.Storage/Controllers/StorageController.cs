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
        var contextAccount = HttpContext.GetContextAccount()!;

        return Ok(await _mediator.Send(new ListStorageByFolderIdCommand
        {
            AccountId = contextAccount.AccountId,
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
    /// <response code="403">When parent folder is not owned by user.</response>
    /// <response code="404">When parent folder is NOT Found.</response>
    [HttpPost("folders")]
    [KDRFCAuthorization]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BlobProjection))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> CreateFolderAsync(CreateBlobFolderRequest request)
    {
        var contextAccount = HttpContext.GetContextAccount()!;

        return Ok(await _mediator.Send(new CreateBlobFolderCommand
        {
            AccountId = contextAccount.AccountId,
            ParentFolderId = request.ParentFolderId,
            FolderName = request.FolderName
        }));
    }

    /// <summary>
    ///     Get Blob Projection(Detail) information.
    /// </summary>
    /// <param name="blobId">Target blob ID to get information.</param>
    /// <response code="200">When successfully got blob information.</response>
    /// <response code="401">When user's credential information is not correct.</response>
    /// <response code="403">When target blob is not user's one.</response>
    /// <response code="404">When parent folder is NOT Found.</response>
    [HttpGet("{blobId}")]
    [KDRFCAuthorization]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BlobProjection))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> GetBlobDetailsAsync(string blobId)
    {
        var contextAccount = HttpContext.GetContextAccount()!;

        return Ok(await _mediator.Send(new GetBlobDetailCommand
        {
            AccountId = contextAccount.AccountId,
            BlobId = blobId
        }));
    }

    /// <summary>
    ///     Upload blob file to storage.
    /// </summary>
    /// <param name="blobFileRequest">A Form, Blob File Request</param>
    /// <returns></returns>
    /// <response code="200">When successfully uploaded blob information.</response>
    /// <response code="400">When parentFolder is not actually folder.</response>
    /// <response code="401">When user's credential information is not correct.</response>
    /// <response code="403">When target blob is not user's one.</response>
    /// <response code="404">When parent folder is NOT Found.</response>
    [HttpPost("upload")]
    [KDRFCAuthorization]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BlobProjection))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> UploadBlobFileAsync([FromForm] CreateBlobFileRequest blobFileRequest)
    {
        var contextAccount = HttpContext.GetContextAccount()!;

        return Ok(await _mediator.Send(new CreateBlobFileCommand
        {
            AccountId = contextAccount.AccountId,
            ParentFolderId = blobFileRequest.ParentFolderId,
            FileMimeType = blobFileRequest.FileContents.ContentType,
            FileContent = blobFileRequest.FileContents.OpenReadStream(),
            FileName = blobFileRequest.FileContents.FileName
        }));
    }
}