using Jacana.Clinical.Application.Abstractions;
using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.Clinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Jacana.SharedKernel.Infrastructure.Persistence;

namespace Jacana.Clinical.Infrastructure.Repositories;

public sealed class ConsultationRepository(ClinicalDbContext db) : IConsultationRepository
{
    public async Task<Consultation?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Consultations
            .Include(c => c.Diagnoses)
            .Include(c => c.Notes)
            .Include(c => c.LabOrders)
            .Include(c => c.PrescriptionOrders)
            .Include(c => c.Referrals)
            .Include(c => c.Documentation)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task AddAsync(Consultation consultation, CancellationToken ct = default)
    {
        db.Consultations.Add(consultation);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Consultation consultation, CancellationToken ct = default)
    {
        // Aggregate already tracked from GetByIdAsync. New children carry
        // client-generated keys; EF DetectChanges would classify them as Modified
        // (phantom UPDATE, 0 rows). Mark them Added explicitly while still Detached.
        db.MarkNewChildrenAdded(consultation);

        // Documentation is a 1:1 reference navigation (not a collection), so the
        // collection walker above does not see it. A brand-new document would
        // otherwise be classified Modified → phantom UPDATE → concurrency error.
        if (consultation.Documentation is not null)
        {
            var entry = db.Entry(consultation.Documentation);
            if (entry.State == EntityState.Detached)
                entry.State = EntityState.Added;
        }

        return Task.CompletedTask;
    }

    public async Task<ConsultationDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var c = await db.Consultations.AsNoTracking()
            .Include(x => x.Diagnoses)
            .Include(x => x.Notes)
            .Include(x => x.Referrals)
            .Include(x => x.Documentation)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (c is null) return null;

        return new ConsultationDetailDto(
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

    public async Task<IReadOnlyList<ConsultationSummaryDto>> GetByPatientAsync(Guid patientId, CancellationToken ct = default)
        => await db.Consultations.AsNoTracking()
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.StartedAtUtc)
            .Select(c => new ConsultationSummaryDto(
                c.Id, c.PatientId, c.ClinicianUserId, c.Status.ToString(),
                c.StartedAtUtc, c.CompletedAtUtc))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ConsultationSummaryDto>> SearchAsync(
        string? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Consultations.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<ConsultationStatus>(status, true, out var parsed))
            query = query.Where(c => c.Status == parsed);

        return await query
            .OrderByDescending(c => c.StartedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ConsultationSummaryDto(
                c.Id, c.PatientId, c.ClinicianUserId, c.Status.ToString(),
                c.StartedAtUtc, c.CompletedAtUtc))
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? status, CancellationToken ct = default)
    {
        var query = db.Consultations.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<ConsultationStatus>(status, true, out var parsed))
            query = query.Where(c => c.Status == parsed);
        return query.CountAsync(ct);
    }

    public async Task<PatientClinicalHistoryDto?> GetPatientHistoryAsync(Guid patientId, CancellationToken ct = default)
    {
        var consultations = await db.Consultations.AsNoTracking()
            .Where(c => c.PatientId == patientId)
            .Select(c => new { c.Id })
            .ToListAsync(ct);

        if (consultations.Count == 0) return null;

        var summaries = await GetByPatientAsync(patientId, ct);

        var diagnoses = await db.Consultations.AsNoTracking()
            .Where(c => c.PatientId == patientId)
            .SelectMany(c => c.Diagnoses)
            .Select(d => new DiagnosisDto(d.IcdCode, d.Description, d.IsPrimary))
            .ToListAsync(ct);

        var notes = await db.Consultations.AsNoTracking()
            .Where(c => c.PatientId == patientId)
            .SelectMany(c => c.Notes)
            .Select(n => new ClinicalNoteDto(n.Content, n.AuthorUserId, n.RecordedAtUtc))
            .ToListAsync(ct);

        return new PatientClinicalHistoryDto(patientId, summaries, diagnoses, notes);
    }
}
