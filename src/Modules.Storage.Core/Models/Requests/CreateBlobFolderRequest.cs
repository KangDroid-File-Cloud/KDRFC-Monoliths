namespace Modules.Storage.Core.Models.Requests;

public class CreateBlobFolderRequest
{
    /// <summary>
    ///     Parent Folder Id(Where this blob is located?)
    /// </summary>
    public string ParentFolderId { get; set; }

    /// <summary>
    ///     Folder Name
    /// </summary>
    /// <example>TestFolderName</example>
    public string FolderName { get; set; }
}