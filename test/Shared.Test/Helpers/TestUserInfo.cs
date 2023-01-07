using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Responses;

namespace Shared.Test.Helpers;

public class TestUserInfo
{
    // Private Backing Field
    private RegisterAccountCommand _registerAccountCommand;

    public AccessTokenResponse AccessToken { get; set; }
    public LoginCommand LoginCommand { get; set; }

    public RegisterAccountCommand RegisterAccountCommand
    {
        get => _registerAccountCommand;
        set
        {
            _registerAccountCommand = value;
            LoginCommand = new LoginCommand
            {
                Email = value.Email,
                AuthCode = value.AuthCode,
                AuthenticationProvider = value.AuthenticationProvider
            };
        }
    }
}