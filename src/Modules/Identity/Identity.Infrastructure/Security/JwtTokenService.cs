using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Jacana.Identity.Application.Abstractions;
using Jacana.SharedKernel.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Jacana.Identity.Infrastructure.Security;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 15;
    public int RefreshTokenDays { get; init; } = 7;
}

/// <summary>
/// JWT issuer. One token format for both delivery mechanisms (cookie + bearer);
/// the access token is short-lived, the refresh token is a random opaque value.
/// </summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public (string AccessToken, string RefreshToken) Generate(
        Guid userId, Guid facilityId,
        IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtClaimTypes.UserId, userId.ToString()),
            new(JwtClaimTypes.FacilityId, facilityId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(JwtClaimTypes.Role, r)));
        claims.AddRange(permissions.Select(p => new Claim(JwtClaimTypes.Permission, p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return (accessToken, refreshToken);
    }
}
