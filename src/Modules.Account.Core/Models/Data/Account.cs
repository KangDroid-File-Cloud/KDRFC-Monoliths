namespace Modules.Account.Core.Models.Data;

public class Account
{
    public string Id { get; set; }
    public string NickName { get; set; }
    public string Email { get; set; }

    public List<Credential> Credentials { get; set; }
}