namespace Jacana.Reporting.Application.DTOs;

/// <summary>Patient registrations grouped by calendar day (date rendered as yyyy-MM-dd).</summary>
public sealed record DailyRegistrationsReportDto(
    string Date,
    int Registrations);

/// <summary>Revenue grouped by service code.</summary>
public sealed record RevenueByServiceReportDto(
    string ServiceCode,
    string Description,
    decimal TotalRevenue);

/// <summary>Current on-hand stock per drug.</summary>
public sealed record StockLevelReportDto(
    Guid DrugId,
    string DrugCode,
    string DrugName,
    int QuantityOnHand);

/// <summary>SHA claims grouped by status.</summary>
public sealed record ShaClaimStatusReportDto(
    string Status,
    int ClaimCount,
    decimal TotalAmount);

/// <summary>Consultation workload per clinician.</summary>
public sealed record ClinicianWorkloadDto(
    Guid ClinicianUserId,
    int ConsultationCount);

/// <summary>Facility-wide dashboard summary.</summary>
public sealed record FacilityDashboardSummaryDto(
    int TotalPatients,
    int OpenAdmissions,
    decimal TotalRevenue,
    int PendingLabOrders,
    int LowStockItems);
