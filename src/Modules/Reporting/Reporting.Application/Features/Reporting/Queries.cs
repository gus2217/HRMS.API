using Jacana.Reporting.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Reporting.Application.Features.Reporting;

public sealed record DailyRegistrationsReportQuery(DateOnly From, DateOnly To)
    : IQuery<Result<IReadOnlyList<DailyRegistrationsReportDto>>>;

public sealed record RevenueByServiceReportQuery()
    : IQuery<Result<IReadOnlyList<RevenueByServiceReportDto>>>;

public sealed record StockLevelReportQuery()
    : IQuery<Result<IReadOnlyList<StockLevelReportDto>>>;

public sealed record ShaClaimStatusReportQuery()
    : IQuery<Result<IReadOnlyList<ShaClaimStatusReportDto>>>;

public sealed record ClinicianWorkloadQuery()
    : IQuery<Result<IReadOnlyList<ClinicianWorkloadDto>>>;

public sealed record FacilityDashboardSummaryQuery()
    : IQuery<Result<FacilityDashboardSummaryDto>>;
