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

public sealed record SaveDocumentationCommand(
    Guid ConsultationId,
    ClinicalDocumentationDataInput Data)
    : ICommand<Result<ConsultationDetailDto>>;

public sealed record CreateReferralCommand(
    Guid ConsultationId,
    string ReferredToFacility,
    string? ReferredToUnit,
    string Reason,
    Domain.ReferralPriority Priority,
    string? Notes)
    : ICommand<Result<ConsultationDetailDto>>;

public sealed record AttachLabOrderCommand(
    Guid ConsultationId,
    Guid LabOrderId,
    string StatusSnapshot)
    : ICommand<Result<ConsultationDetailDto>>;

/// <summary>Section values for the structured clinical document (all optional).</summary>
public sealed record ClinicalDocumentationDataInput(
    string? ChiefComplaint,
    string? HistoryOfPresentingIllness,
    string? PastMedicalHistory,
    string? PastSurgicalHistory,
    string? FamilyHistory,
    string? SocialHistory,
    string? GynaecologicalHistory,
    string? ObstetricHistory,
    string? DrugHistory,
    string? RosGeneral,
    string? RosCardiovascular,
    string? RosRespiratory,
    string? RosGastrointestinal,
    string? RosGenitourinary,
    string? RosMusculoskeletal,
    string? RosNeurological,
    string? RosDermatological,
    string? RosEntEyes,
    string? RosEndocrine,
    string? ExamGeneralAppearance,
    string? ExamHeadAndNeck,
    string? ExamCardiovascular,
    string? ExamRespiratory,
    string? ExamAbdominal,
    string? ExamGenitourinary,
    string? ExamMusculoskeletal,
    string? ExamNeurological,
    string? ExamSkin,
    string? ExamLymphatic);

public sealed record CompleteConsultationCommand(
    Guid ConsultationId)
    : ICommand<Result<ConsultationDetailDto>>;
