namespace Modules.Account.Core.Models.Data;

public enum AuthenticationProvider
{
    Self
}

public class Credential
{
    public AuthenticationProvider AuthenticationProvider { get; set; }
    public string ProviderId { get; set; }
    public string? Key { get; set; }
}