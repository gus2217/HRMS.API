namespace Jacana.SharedKernel.Infrastructure;

/// <summary>
/// Design-time values used by <c>IDesignTimeDbContextFactory</c> implementations so
/// `dotnet ef migrations add` can build models without a running host or a live DB.
/// </summary>
public static class DesignTime
{
    /// <summary>Placeholder connection string — migrations generation never connects.</summary>
    public const string ConnectionString =
        "Host=localhost;Port=5432;Database=jacana_hrms;Username=jacana;Password=jacana";

    /// <summary>
    /// 32-byte dev key (base64) for the NationalId AES-GCM encryptor. Used only at
    /// design time; production reads the key from the secrets store.
    /// </summary>
    public const string DevEncryptionKey = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
}
