using Jacana.Reporting.Application.Abstractions;
using Jacana.Reporting.Application.DTOs;
using Jacana.Reporting.Application.Features.Reporting;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Reporting.Application.Features.Reporting.Handlers;

public sealed class DailyRegistrationsReportQueryHandler(IReportingReadRepository reports)
    : IRequestHandler<DailyRegistrationsReportQuery, Result<IReadOnlyList<DailyRegistrationsReportDto>>>
{
    public async Task<Result<IReadOnlyList<DailyRegistrationsReportDto>>> Handle(
        DailyRegistrationsReportQuery request, CancellationToken ct)
    {
        var data = await reports.DailyRegistrationsAsync(request.From, request.To, ct);
        return Result.Success(data);
    }
}

public sealed class RevenueByServiceReportQueryHandler(IReportingReadRepository reports)
    : IRequestHandler<RevenueByServiceReportQuery, Result<IReadOnlyList<RevenueByServiceReportDto>>>
{
    public async Task<Result<IReadOnlyList<RevenueByServiceReportDto>>> Handle(
        RevenueByServiceReportQuery request, CancellationToken ct)
    {
        var data = await reports.RevenueByServiceAsync(ct);
        return Result.Success(data);
    }
}

public sealed class StockLevelReportQueryHandler(IReportingReadRepository reports)
    : IRequestHandler<StockLevelReportQuery, Result<IReadOnlyList<StockLevelReportDto>>>
{
    public async Task<Result<IReadOnlyList<StockLevelReportDto>>> Handle(
        StockLevelReportQuery request, CancellationToken ct)
    {
        var data = await reports.StockLevelsAsync(ct);
        return Result.Success(data);
    }
}

public sealed class ShaClaimStatusReportQueryHandler(IReportingReadRepository reports)
    : IRequestHandler<ShaClaimStatusReportQuery, Result<IReadOnlyList<ShaClaimStatusReportDto>>>
{
    public async Task<Result<IReadOnlyList<ShaClaimStatusReportDto>>> Handle(
        ShaClaimStatusReportQuery request, CancellationToken ct)
    {
        var data = await reports.ShaClaimStatusAsync(ct);
        return Result.Success(data);
    }
}

public sealed class ClinicianWorkloadQueryHandler(IReportingReadRepository reports)
    : IRequestHandler<ClinicianWorkloadQuery, Result<IReadOnlyList<ClinicianWorkloadDto>>>
{
    public async Task<Result<IReadOnlyList<ClinicianWorkloadDto>>> Handle(
        ClinicianWorkloadQuery request, CancellationToken ct)
    {
        var data = await reports.ClinicianWorkloadAsync(ct);
        return Result.Success(data);
    }
}

public sealed class FacilityDashboardSummaryQueryHandler(IReportingReadRepository reports)
    : IRequestHandler<FacilityDashboardSummaryQuery, Result<FacilityDashboardSummaryDto>>
{
    public async Task<Result<FacilityDashboardSummaryDto>> Handle(
        FacilityDashboardSummaryQuery request, CancellationToken ct)
    {
        var summary = await reports.DashboardSummaryAsync(ct);
        return summary is null ? Error.NotFound("Dashboard summary is unavailable.") : summary;
    }
}
