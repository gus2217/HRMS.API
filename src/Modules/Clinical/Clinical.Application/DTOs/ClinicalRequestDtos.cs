namespace Jacana.Clinical.Application.DTOs;

// HTTP request bindings for the clinical endpoints (framework-agnostic records).

public sealed record StartConsultationRequestDto(Guid PatientId, Guid ClinicianUserId);

public sealed record RecordTriageRequestDto(
    decimal? TemperatureCelsius,
    string? BloodPressure,
    int? PulseRate,
    int? RespiratoryRate,
    decimal? WeightKg);

public sealed record RecordDiagnosisRequestDto(string IcdCode, string Description, bool IsPrimary);

public sealed record AddClinicalNoteRequestDto(string Content);

public sealed record SaveDocumentationRequestDto(
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

public sealed record CreateReferralRequestDto(
    string ReferredToFacility,
    string? ReferredToUnit,
    string Reason,
    string Priority,
    string? Notes);
