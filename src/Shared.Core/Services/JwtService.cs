using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Shared.Core.Services;

public static class KDRFCCommonClaimName
{
    public const string AuthenticationProviderId = "provider";
    public const string Nickname = "nickname";
    public const string Email = "email";
}

public interface IJwtService
{
    string GenerateJwt(List<Claim>? claims, DateTime? dateTime = null);

    JwtSecurityToken? ValidateJwt(string jwt, bool validateLifetime = true);
}

public class JwtService : IJwtService
{
    private const string Issuer = "KDRFC";
    private const string Audience = "KDRFC";

    // Logger
    private readonly ILogger _logger;

    // JWT Symmetric Key Information
    private readonly SymmetricSecurityKey _securityKey;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSecurityKey"]));
        _logger = logger;
    }

    public string GenerateJwt(List<Claim>? claims, DateTime? expires = null)
    {
        // Default: 60m
        expires ??= DateTime.UtcNow.AddHours(1);

        var credential = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(Issuer, Audience, claims, DateTime.UtcNow, expires, credential);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public JwtSecurityToken? ValidateJwt(string jwt, bool validateLifetime = true)
    {
        JwtSecurityToken? validatedSecurityToken = null;
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateLifetime,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Issuer,
            ValidAudience = Audience,
            IssuerSigningKey = _securityKey,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(jwt, tokenValidationParameters, out var validatedToken);

            validatedSecurityToken = (JwtSecurityToken)validatedToken;
        }
        catch (Exception e)
        {
            _logger.LogWarning("[JwtService] Failed to validate JWT: {message}", e.Message);
            return null;
        }

        return validatedSecurityToken;
    }
}