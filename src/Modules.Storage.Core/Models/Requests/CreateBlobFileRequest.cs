using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Modules.Storage.Core.Models.Requests;

public class CreateBlobFileRequest
{
    /// <summary>
    ///     Parent Folder Id - Where file stored.
    /// </summary>
    [Required]
    public string ParentFolderId { get; set; }

    /// <summary>
    ///     File Contents, Via FormFile.
    /// </summary>
    [Required]
    public IFormFile FileContents { get; set; }
}