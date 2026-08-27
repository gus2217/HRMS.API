namespace Jacana.Inpatient.Application.DTOs;

// HTTP request bindings for inpatient endpoints.

public sealed record AdmitPatientRequestDto(
    Guid PatientId,
    Guid AdmittingClinicianUserId,
    string WardName,
    string BedNumber);

public sealed record AddWardNoteRequestDto(string Content);
