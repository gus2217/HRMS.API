using Jacana.Reporting.Application.DTOs;

namespace Jacana.Reporting.Application.Abstractions;

/// <summary>
/// Read-only reporting queries. These project straight to DTOs against other modules'
/// schemas — read access only, never writes.
/// </summary>
public interface IReportingReadRepository
{
    Task<IReadOnlyList<DailyRegistrationsReportDto>> DailyRegistrationsAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<IReadOnlyList<RevenueByServiceReportDto>> RevenueByServiceAsync(CancellationToken ct = default);
    Task<IReadOnlyList<StockLevelReportDto>> StockLevelsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ShaClaimStatusReportDto>> ShaClaimStatusAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ClinicianWorkloadDto>> ClinicianWorkloadAsync(CancellationToken ct = default);
    Task<FacilityDashboardSummaryDto?> DashboardSummaryAsync(CancellationToken ct = default);
}
