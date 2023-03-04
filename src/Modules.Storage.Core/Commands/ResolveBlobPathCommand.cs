using MediatR;
using Modules.Storage.Core.Models.Responses;

namespace Modules.Storage.Core.Commands;

public class ResolveBlobPathCommand : IRequest<List<BlobProjection>>
{
    /// <summary>
    ///     Target Blob ID to resolve path.
    /// </summary>
    public string TargetBlobId { get; set; }

    /// <summary>
    ///     Request User
    /// </summary>
    public string UserId { get; set; }
}