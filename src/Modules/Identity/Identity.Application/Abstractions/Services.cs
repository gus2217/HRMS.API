namespace Jacana.Identity.Application.Abstractions;

/// <summary>Argon2id password hashing. Never a fast hash.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

/// <summary>Issues JWTs (access + refresh). Same token format for cookie and bearer delivery.</summary>
public interface ITokenService
{
    (string AccessToken, string RefreshToken) Generate(Guid userId, Guid facilityId,
        IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions);
}

/// <summary>TOTP (RFC 6238) for opt-in two-factor authentication.</summary>
public interface ITotpService
{
    string GenerateSecret();
    string GenerateQrCodeUri(string secret, string accountName);
    bool Validate(string secret, string code);
}
