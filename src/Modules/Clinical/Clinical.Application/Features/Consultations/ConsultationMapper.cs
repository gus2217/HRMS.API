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
            c.Notes.Select(n => new ClinicalNoteDto(n.Content, n.AuthorUserId, n.RecordedAtUtc)).ToArray());
}
