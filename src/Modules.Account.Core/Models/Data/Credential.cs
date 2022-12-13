using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Modules.Account.Core.Models.Data;

[JsonConverter(typeof(StringEnumConverter))]
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