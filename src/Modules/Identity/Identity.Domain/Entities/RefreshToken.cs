namespace Jacana.Identity.Domain;

/// <summary>
/// A refresh token. Only the SHA-256 hash is persisted; the raw token is returned
/// once to the client and never stored. Rotated on every use.
/// </summary>
public sealed class RefreshToken
{
    private RefreshToken() { } // EF

    private RefreshToken(Guid id, Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public bool IsRevoked { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsExpired(DateTime nowUtc) => nowUtc >= ExpiresAtUtc;
    public bool IsActive(DateTime nowUtc) => !IsRevoked && !IsExpired(nowUtc);

    public static RefreshToken Create(Guid id, Guid userId, string tokenHash, DateTime expiresAtUtc)
        => new(id, userId, tokenHash, expiresAtUtc);

    public void Revoke(Guid? replacedByTokenId = null)
    {
        IsRevoked = true;
        ReplacedByTokenId = replacedByTokenId;
    }
}
