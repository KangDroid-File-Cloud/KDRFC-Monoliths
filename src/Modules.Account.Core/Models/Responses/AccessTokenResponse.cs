using System.ComponentModel.DataAnnotations;

namespace Modules.Account.Core.Models.Responses;

public class AccessTokenResponse
{
    [Required]
    public string AccessToken { get; set; }

    [Required]
    public string RefreshToken { get; set; }
}