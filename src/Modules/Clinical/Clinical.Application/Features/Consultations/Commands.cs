using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Application.Features.Consultations;

public sealed record StartConsultationCommand(
    Guid PatientId,
    Guid ClinicianUserId)
    : ICommand<Result<ConsultationDetailDto>>;

public sealed record RecordTriageCommand(
    Guid ConsultationId,
    decimal? TemperatureCelsius,
    string? BloodPressure,
    int? PulseRate,
    int? RespiratoryRate,
    decimal? WeightKg)
    : ICommand<Result<ConsultationDetailDto>>;

public sealed record BeginClinicalPhaseCommand(
    Guid ConsultationId)
    : ICommand<Result<ConsultationDetailDto>>;

public sealed record RecordDiagnosisCommand(
    Guid ConsultationId,
    string IcdCode,
    string Description,
    bool IsPrimary)
    : ICommand<Result<ConsultationDetailDto>>;

public sealed record AddClinicalNoteCommand(
    Guid ConsultationId,
    string Content)
    : ICommand<Result<ConsultationDetailDto>>;

public sealed record AttachLabOrderCommand(
    Guid ConsultationId,
    Guid LabOrderId,
    string StatusSnapshot)
    : ICommand<Result<ConsultationDetailDto>>;

public sealed record CompleteConsultationCommand(
    Guid ConsultationId)
    : ICommand<Result<ConsultationDetailDto>>;
