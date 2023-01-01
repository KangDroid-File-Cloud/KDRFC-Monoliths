using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Modules.Storage.Core.Models;

/// <summary>
///     C# representation of blob file metadata
/// </summary>
public class BlobFile
{
    /// <summary>
    ///     Blob File ID
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public ObjectId Id { get; set; }

    /// <summary>
    ///     File Owner(Typically UserId)
    /// </summary>
    [BsonElement("ownerId")]
    public string OwnerId { get; set; }

    /// <summary>
    ///     Parent Folder ID
    /// </summary>
    [BsonElement("parentFolderId")]
    public string ParentFolderId { get; set; }

    /// <summary>
    ///     Blob File Type - Might be file or folder.
    /// </summary>
    [BsonElement("blobFileType")]
    public BlobFileType BlobFileType { get; set; }
}

public enum BlobFileType
{
    File,
    Folder
}