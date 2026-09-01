using Jacana.SharedKernel.Application.Abstractions;
using Npgsql;

namespace Jacana.SharedKernel.Infrastructure.Services;

/// <summary>
/// Raw-Npgsql lookup against the billing schema. An invoice is "outstanding" when
/// it is Issued or PartiallyPaid — Draft (not yet billed), Paid, Cancelled and
/// WrittenOff are all considered settled for the discharge gate.
/// </summary>
public sealed class BillingStatusLookup(string connectionString) : IBillingStatusLookup
{
    public async Task<bool> IsBillClearedAsync(Guid patientId, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            """
            SELECT NOT EXISTS (
                SELECT 1
                FROM billing.invoices
                WHERE "PatientId" = @patientId
                  AND "Status" IN ('Issued', 'PartiallyPaid')
                  AND "IsDeleted" = FALSE
            )
            """, conn);
        cmd.Parameters.AddWithValue("patientId", patientId);

        var result = await cmd.ExecuteScalarAsync(ct);
        return result is bool b && b;
    }
}
