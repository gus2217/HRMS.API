namespace Jacana.Clinical.Application.DTOs;

/// <summary>HTTP request binding for queueing a patient.</summary>
public sealed record CreateQueueEntryRequestDto(
    Guid PatientId,
    string ClinicType,
    string Priority,
    string? Notes);
