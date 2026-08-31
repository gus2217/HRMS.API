using Jacana.SharedKernel.Application.Abstractions;
using Npgsql;
using NpgsqlTypes;

namespace Jacana.SharedKernel.Infrastructure.Services;

/// <summary>
/// Raw-Npgsql user identity lookup against the identity schema, mirroring
/// <see cref="PatientIdentityLookup"/>. Resolves clinician/receptionist names for
/// cross-module display without leaking the Identity module's entities.
/// </summary>
public sealed class UserIdentityLookup(string connectionString) : IUserIdentityLookup
{
    public async Task<IReadOnlyDictionary<Guid, UserIdentityDto>> GetIdentitiesAsync(
        IReadOnlyCollection<Guid> userIds, CancellationToken ct = default)
    {
        if (userIds.Count == 0)
            return new Dictionary<Guid, UserIdentityDto>();

        var ids = userIds.Distinct().ToArray();
        var result = new Dictionary<Guid, UserIdentityDto>(ids.Length);

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            """
            SELECT "Id", "FullName"
            FROM identity.users
            WHERE "Id" = ANY(@ids)
            """, conn);
        cmd.Parameters.AddWithValue("ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid, ids);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var id = reader.GetGuid(0);
            result[id] = new UserIdentityDto(id, reader.GetString(1));
        }
        return result;
    }
}
