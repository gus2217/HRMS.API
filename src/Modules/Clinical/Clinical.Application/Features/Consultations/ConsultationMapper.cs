using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;

namespace Jacana.Clinical.Application.Features.Consultations;

/// <summary>
/// Maps an in-memory <see cref="Consultation"/> aggregate to its detail DTO.
/// Handlers use this after mutating an aggregate instead of re-querying the
/// database (the unit-of-work transaction has not committed yet at that point).
/// </summary>
internal static class ConsultationMapper
{
    public static ConsultationDetailDto ToDetail(Consultation c) =>
        new(
            c.Id, c.PatientId, c.ClinicianUserId, c.Status.ToString(),
            c.StartedAtUtc, c.CompletedAtUtc,
            c.Triage is null ? null : new TriageDataDto(
                c.Triage.TemperatureCelsius, c.Triage.BloodPressure, c.Triage.PulseRate,
                c.Triage.RespiratoryRate, c.Triage.WeightKg),
            c.Diagnoses.Select(d => new DiagnosisDto(d.IcdCode, d.Description, d.IsPrimary)).ToArray(),
            c.Notes.Select(n => new ClinicalNoteDto(n.Content, n.AuthorUserId, n.RecordedAtUtc)).ToArray(),
            c.Documentation is null ? null : new ClinicalDocumentationDto(
                c.Documentation.ChiefComplaint,
                c.Documentation.HistoryOfPresentingIllness,
                c.Documentation.PastMedicalHistory,
                c.Documentation.PastSurgicalHistory,
                c.Documentation.FamilyHistory,
                c.Documentation.SocialHistory,
                c.Documentation.GynaecologicalHistory,
                c.Documentation.ObstetricHistory,
                c.Documentation.DrugHistory,
                c.Documentation.RosGeneral,
                c.Documentation.RosCardiovascular,
                c.Documentation.RosRespiratory,
                c.Documentation.RosGastrointestinal,
                c.Documentation.RosGenitourinary,
                c.Documentation.RosMusculoskeletal,
                c.Documentation.RosNeurological,
                c.Documentation.RosDermatological,
                c.Documentation.RosEntEyes,
                c.Documentation.RosEndocrine,
                c.Documentation.ExamGeneralAppearance,
                c.Documentation.ExamHeadAndNeck,
                c.Documentation.ExamCardiovascular,
                c.Documentation.ExamRespiratory,
                c.Documentation.ExamAbdominal,
                c.Documentation.ExamGenitourinary,
                c.Documentation.ExamMusculoskeletal,
                c.Documentation.ExamNeurological,
                c.Documentation.ExamSkin,
                c.Documentation.ExamLymphatic,
                c.Documentation.LastSavedAtUtc,
                c.Documentation.LastSavedByUserId),
            c.Referrals.Select(r => new ReferralDto(
                r.Id, r.ReferredToFacility, r.ReferredToUnit, r.Reason,
                r.Priority.ToString(), r.Status.ToString(), r.Notes, r.ReferredAtUtc)).ToArray());
}
