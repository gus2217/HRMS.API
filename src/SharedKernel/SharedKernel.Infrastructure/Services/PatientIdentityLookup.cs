using Jacana.SharedKernel.Application.Abstractions;
using Npgsql;
using NpgsqlTypes;

namespace Jacana.SharedKernel.Infrastructure.Services;

/// <summary>
/// Dapper-free patient identity lookup against the patient schema. Uses the raw
/// Npgsql connection (same connection string as the module DbContexts) so list
/// endpoints in any module can render patient numbers and full names.
/// </summary>
public sealed class PatientIdentityLookup(string connectionString) : IPatientIdentityLookup
{
    public async Task<IReadOnlyDictionary<Guid, PatientIdentityDto>> GetIdentitiesAsync(
        IReadOnlyCollection<Guid> patientIds, CancellationToken ct = default)
    {
        if (patientIds.Count == 0)
            return new Dictionary<Guid, PatientIdentityDto>();

        var ids = patientIds.Distinct().ToArray();
        var result = new Dictionary<Guid, PatientIdentityDto>(ids.Length);

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            """
            SELECT "Id", "PatientNumber", "FirstName", "LastName"
            FROM patient.patients
            WHERE "Id" = ANY(@ids)
            """, conn);
        cmd.Parameters.AddWithValue("ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid, ids);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var id = reader.GetGuid(0);
            result[id] = new PatientIdentityDto(
                id,
                reader.GetString(1),
                $"{reader.GetString(2)} {reader.GetString(3)}");
        }
        return result;
    }
}
