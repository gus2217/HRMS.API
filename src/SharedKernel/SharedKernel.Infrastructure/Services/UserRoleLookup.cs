using Jacana.SharedKernel.Application.Abstractions;
using Npgsql;
using NpgsqlTypes;

namespace Jacana.SharedKernel.Infrastructure.Services;

/// <summary>
/// Raw-Npgsql role → user-ID lookup against the identity schema (users joined to
/// user_roles → roles). Returns only active users, so suspended/deactivated
/// accounts never receive notifications.
/// </summary>
public sealed class UserRoleLookup(string connectionString) : IUserRoleLookup
{
    public async Task<IReadOnlyCollection<Guid>> GetUserIdsByRolesAsync(
        IReadOnlyCollection<string> roleNames, CancellationToken ct = default)
    {
        if (roleNames.Count == 0)
            return [];

        var names = roleNames.Distinct().ToArray();
        var result = new List<Guid>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            """
            SELECT DISTINCT u."Id"
            FROM identity.users u
            INNER JOIN identity.user_roles ur ON ur."UserId" = u."Id"
            INNER JOIN identity.roles r ON r."Id" = ur."RoleId"
            WHERE r."Name" = ANY(@names) AND u."Status" = 'Active'
            """, conn);
        cmd.Parameters.AddWithValue("names", NpgsqlDbType.Array | NpgsqlDbType.Text, names);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            result.Add(reader.GetGuid(0));

        return result;
    }
}
