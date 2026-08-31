using Dapper;
using Jacana.Reporting.Application.Abstractions;
using Jacana.Reporting.Application.DTOs;
using Npgsql;

namespace Jacana.Reporting.Infrastructure.Repositories;

/// <summary>
/// Dapper-based read-only reporting queries. Projects straight to DTOs against other
/// modules' schemas. No EF entities, no DbContext, no write path.
/// </summary>
public sealed class ReportingReadRepository(string connectionString) : IReportingReadRepository
{
    public async Task<IReadOnlyList<DailyRegistrationsReportDto>> DailyRegistrationsAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        // Dapper's bundled type handlers predate DateOnly — pass yyyy-MM-dd strings
        // and cast in SQL instead of binding DateOnly parameters.
        return (await conn.QueryAsync<DailyRegistrationsReportDto>(new CommandDefinition(
            """
            SELECT TO_CHAR(CAST("CreatedAtUtc" AS date), 'YYYY-MM-DD') AS "Date",
                   CAST(COUNT(*) AS integer) AS "Registrations"
            FROM patient.patients
            WHERE CAST("CreatedAtUtc" AS date) BETWEEN CAST(@from AS date) AND CAST(@to AS date)
            GROUP BY TO_CHAR(CAST("CreatedAtUtc" AS date), 'YYYY-MM-DD')
            ORDER BY 1
            """,
            new { from = from.ToString("yyyy-MM-dd"), to = to.ToString("yyyy-MM-dd") },
            cancellationToken: ct))).ToList();
    }

    public async Task<IReadOnlyList<RevenueByServiceReportDto>> RevenueByServiceAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        return (await conn.QueryAsync<RevenueByServiceReportDto>(new CommandDefinition(
            """
            SELECT l."ServiceCode", l."Description", SUM(l."UnitPrice" * l."Quantity") AS "TotalRevenue"
            FROM billing.invoice_lines l
            INNER JOIN billing.invoices i ON i."Id" = l."InvoiceId"
            WHERE i."Status" NOT IN ('Cancelled', 'Draft')
            GROUP BY l."ServiceCode", l."Description"
            ORDER BY "TotalRevenue" DESC
            """,
            cancellationToken: ct))).ToList();
    }

    public async Task<IReadOnlyList<StockLevelReportDto>> StockLevelsAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        return (await conn.QueryAsync<StockLevelReportDto>(new CommandDefinition(
            """
            SELECT d."Id" AS "DrugId", d."Code" AS "DrugCode", d."Name" AS "DrugName",
                   CAST(COALESCE(SUM(b."QuantityOnHand"), 0) AS integer) AS "QuantityOnHand"
            FROM inventory.drugs d
            LEFT JOIN inventory.stock_batches b ON b."DrugId" = d."Id" AND b."QuantityOnHand" > 0
            WHERE d."IsDeleted" = false
            GROUP BY d."Id", d."Code", d."Name"
            ORDER BY "QuantityOnHand" ASC
            """,
            cancellationToken: ct))).ToList();
    }

    public async Task<IReadOnlyList<ShaClaimStatusReportDto>> ShaClaimStatusAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        return (await conn.QueryAsync<ShaClaimStatusReportDto>(new CommandDefinition(
            """
            SELECT c."Status", CAST(COUNT(*) AS integer) AS "ClaimCount",
                   CAST(COALESCE(SUM(l."UnitPrice" * l."Quantity"), 0) AS numeric(18,2)) AS "TotalAmount"
            FROM billing.sha_claims c
            INNER JOIN billing.invoices i ON i."Id" = c."InvoiceId"
            INNER JOIN billing.invoice_lines l ON l."InvoiceId" = i."Id"
            GROUP BY c."Status"
            ORDER BY c."Status"
            """,
            cancellationToken: ct))).ToList();
    }

    public async Task<IReadOnlyList<ClinicianWorkloadDto>> ClinicianWorkloadAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        return (await conn.QueryAsync<ClinicianWorkloadDto>(new CommandDefinition(
            """
            SELECT c."ClinicianUserId",
                   COALESCE(u."FullName", 'Unknown clinician') AS "ClinicianName",
                   CAST(COUNT(*) AS integer) AS "ConsultationCount"
            FROM clinical.consultations c
            LEFT JOIN identity.users u ON u."Id" = c."ClinicianUserId"
            WHERE c."IsDeleted" = false
            GROUP BY c."ClinicianUserId", u."FullName"
            ORDER BY "ConsultationCount" DESC
            """,
            cancellationToken: ct))).ToList();
    }

    public async Task<FacilityDashboardSummaryDto?> DashboardSummaryAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);

        var totalPatients = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM patient.patients WHERE \"IsDeleted\" = false");

        var openAdmissions = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM inpatient.admissions WHERE \"Status\" <> 'Discharged' AND \"IsDeleted\" = false");

        var totalRevenue = await conn.ExecuteScalarAsync<decimal>(
            """
            SELECT COALESCE(SUM(l."UnitPrice" * l."Quantity"), 0)
            FROM billing.invoice_lines l
            INNER JOIN billing.invoices i ON i."Id" = l."InvoiceId"
            WHERE i."Status" = 'Paid' AND i."IsDeleted" = false
            """);

        var pendingLabOrders = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM laboratory.lab_orders WHERE \"Status\" IN ('Pending','InProgress','PartiallyCompleted') AND \"IsDeleted\" = false");

        var lowStockItems = await conn.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*) FROM inventory.drugs d
            WHERE d."IsDeleted" = false AND d."ReorderLevel" > 0
              AND (SELECT COALESCE(SUM(b."QuantityOnHand"), 0) FROM inventory.stock_batches b
                   WHERE b."DrugId" = d."Id" AND b."QuantityOnHand" > 0) <= d."ReorderLevel"
            """);

        return new FacilityDashboardSummaryDto(
            totalPatients, openAdmissions, totalRevenue, pendingLabOrders, lowStockItems);
    }
}
