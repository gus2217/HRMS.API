namespace Jacana.Clinical.Application.DTOs;

/// <summary>List-view row with patient display identity resolved cross-schema.</summary>
public sealed record ConsultationListItemDto(
    Guid Id,
    Guid PatientId,
    string PatientNumber,
    string PatientName,
    Guid ClinicianUserId,
    string Status,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc);

/// <summary>Lean read-model for list/history views.</summary>
public sealed record ConsultationSummaryDto(
    Guid Id,
    Guid PatientId,
    Guid ClinicianUserId,
    string Status,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc);

/// <summary>Full single-record projection.</summary>
public sealed record ConsultationDetailDto(
    Guid Id,
    Guid PatientId,
    Guid ClinicianUserId,
    string Status,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    TriageDataDto? Triage,
    IReadOnlyList<DiagnosisDto> Diagnoses,
    IReadOnlyList<ClinicalNoteDto> Notes,
    ClinicalDocumentationDto? Documentation,
    IReadOnlyList<ReferralDto> Referrals);

public sealed record TriageDataDto(
    decimal? TemperatureCelsius,
    string? BloodPressure,
    int? PulseRate,
    int? RespiratoryRate,
    decimal? WeightKg);

public sealed record DiagnosisDto(string IcdCode, string Description, bool IsPrimary);
public sealed record ClinicalNoteDto(string Content, Guid AuthorUserId, DateTime RecordedAtUtc);

/// <summary>Structured medical documentation (CC → HPI → PMSHX → ROS → Exam).</summary>
public sealed record ClinicalDocumentationDto(
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
    string? ExamLymphatic,
    DateTime? LastSavedAtUtc,
    Guid? LastSavedByUserId);

public sealed record ReferralDto(
    Guid Id,
    string ReferredToFacility,
    string? ReferredToUnit,
    string Reason,
    string Priority,
    string Status,
    string? Notes,
    DateTime ReferredAtUtc);

/// <summary>Read-model of a patient's clinical history across consultations.</summary>
public sealed record PatientClinicalHistoryDto(
    Guid PatientId,
    IReadOnlyList<ConsultationSummaryDto> Consultations,
    IReadOnlyList<DiagnosisDto> Diagnoses,
    IReadOnlyList<ClinicalNoteDto> Notes);
