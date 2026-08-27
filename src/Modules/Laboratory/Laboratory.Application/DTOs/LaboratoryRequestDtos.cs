namespace Jacana.Laboratory.Application.DTOs;

// HTTP request bindings for laboratory endpoints.

public sealed record CreateLabOrderRequestDto(
    Guid PatientId,
    Guid ConsultationId,
    IReadOnlyList<LabTestRequestDto> Tests);

public sealed record LabTestRequestDto(string TestCode, string TestName);

public sealed record RecordLabResultRequestDto(
    Guid TestItemId,
    string? ResultValue,
    string? ResultUnit,
    string? ReferenceRange,
    bool? IsAbnormal);
