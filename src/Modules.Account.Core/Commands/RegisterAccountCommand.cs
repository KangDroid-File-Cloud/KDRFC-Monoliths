using MediatR;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Services;

namespace Modules.Account.Core.Commands;

public class RegisterAccountCommand : IRequest
{
    /// <summary>
    ///     Nickname of User.
    /// </summary>
    /// <example>KangDroid</example>
    public string Nickname { get; set; }

    /// <summary>
    ///     Email of User
    /// </summary>
    /// <example>kangdroid@testhelloworld.com</example>
    public string Email { get; set; }

    /// <summary>
    ///     Authentication Provider - Self, OAuth, etc.
    /// </summary>
    /// <example>Self</example>
    public AuthenticationProvider AuthenticationProvider { get; set; }

    /// <summary>
    ///     Authentication Code.(Password when self.)
    /// </summary>
    public string AuthCode { get; set; }
}

public class RegisterAccountCommandHandler : IRequestHandler<RegisterAccountCommand>
{
    private readonly AuthenticationProviderFactory _authenticationProviderFactory;

    public RegisterAccountCommandHandler(AuthenticationProviderFactory factory)
    {
        _authenticationProviderFactory = factory;
    }

    public async Task<Unit> Handle(RegisterAccountCommand request, CancellationToken cancellationToken)
    {
        var providerFactory = request.AuthenticationProvider switch
        {
            AuthenticationProvider.Self => _authenticationProviderFactory(AuthenticationProvider.Self),
            _ => throw new ArgumentException("Unknown Value", request.AuthenticationProvider.ToString())
        };

        // Create Account(May throw ApiException)
        await providerFactory.CreateAccountAsync(request);
        return Unit.Value;
    }
}