namespace Jacana.Laboratory.Application.DTOs;

public sealed record LabTestItemDto(
    Guid Id,
    string TestCode,
    string TestName,
    string Status,
    string? ResultValue,
    string? ResultUnit,
    string? ReferenceRange,
    bool? IsAbnormal);

public sealed record LabOrderSummaryDto(
    Guid Id,
    Guid PatientId,
    Guid OrderedByUserId,
    string Status,
    DateTime OrderedAtUtc,
    int TestCount);

/// <summary>List-view row with patient display identity resolved cross-schema.</summary>
public sealed record LabOrderListItemDto(
    Guid Id,
    Guid PatientId,
    string PatientNumber,
    string PatientName,
    Guid OrderedByUserId,
    string Status,
    DateTime OrderedAtUtc,
    int TestCount);

public sealed record LabOrderDetailDto(
    Guid Id,
    Guid PatientId,
    Guid ConsultationId,
    Guid OrderedByUserId,
    string Status,
    DateTime OrderedAtUtc,
    IReadOnlyList<LabTestItemDto> Tests);
