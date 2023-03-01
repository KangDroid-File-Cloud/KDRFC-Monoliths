namespace Modules.Storage.Core.Models.Responses;

public class BlobProjection
{
    /// <summary>
    ///     Blob ID
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Blob Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     File Length
    /// </summary>
    /// <remarks>
    ///     Can be 0 when Blob type is Folder.
    /// </remarks>
    public long Length { get; set; }

    /// <summary>
    ///     Date Blob Uploaded
    /// </summary>
    public DateTimeOffset UploadDate { get; set; }

    /// <summary>
    ///     Parent Folder Id.
    /// </summary>
    public string ParentFolderId { get; set; }

    /// <summary>
    ///     Current Blob File Type
    /// </summary>
    public BlobFileType BlobFileType { get; set; }
}