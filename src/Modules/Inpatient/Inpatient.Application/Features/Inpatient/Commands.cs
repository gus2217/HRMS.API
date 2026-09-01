using Jacana.Inpatient.Application.DTOs;
using Jacana.Inpatient.Domain;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Inpatient.Application.Features.Inpatient;

// ── Wards (admin-managed) ────────────────────────────────────────────────

public sealed record CreateWardCommand(string Name, WardType Type, int TotalBeds)
    : ICommand<Result<WardDto>>;

public sealed record UpdateWardCommand(Guid WardId, string Name, WardType Type, int TotalBeds)
    : ICommand<Result<WardDto>>;

public sealed record DeactivateWardCommand(Guid WardId)
    : ICommand<Result<WardDto>>;

// ── Admissions ───────────────────────────────────────────────────────────

public sealed record AdmitPatientCommand(
    Guid PatientId,
    Guid AdmittingClinicianUserId,
    Guid WardId,
    string BedNumber,
    string? AdmittingDiagnosis,
    Guid? AttendingClinicianUserId)
    : ICommand<Result<AdmissionDetailDto>>;

public sealed record DischargePatientCommand(
    Guid AdmissionId)
    : ICommand<Result<AdmissionDetailDto>>;

/// <summary>Transfers an active admission to another ward/bed (capacity-checked).</summary>
public sealed record TransferPatientCommand(
    Guid AdmissionId,
    Guid TargetWardId,
    string BedNumber)
    : ICommand<Result<AdmissionDetailDto>>;

public sealed record AddWardNoteCommand(
    Guid AdmissionId,
    string Content)
    : ICommand<Result<AdmissionDetailDto>>;

public sealed record AddMedicalRecordCommand(
    Guid AdmissionId,
    decimal? TemperatureCelsius,
    int? SystolicBp,
    int? DiastolicBp,
    int? PulseRate,
    int? RespiratoryRate,
    int? OxygenSaturation,
    decimal? WeightKg,
    string? Subjective,
    string? Objective,
    string? Assessment,
    string? Plan)
    : ICommand<Result<AdmissionDetailDto>>;

/// <summary>Uploads a media/image file onto a ward medical record (SOAP note).</summary>
public sealed record AttachMedicalRecordFileCommand(
    Guid MedicalRecordId,
    string FileName,
    string ContentType,
    byte[] Content)
    : ICommand<Result<AdmissionDetailDto>>;
