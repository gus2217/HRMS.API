using Jacana.Clinical.Application.Abstractions;
using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.Clinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Clinical.Infrastructure.Repositories;

public sealed class ConsultationRepository(ClinicalDbContext db) : IConsultationRepository
{
    public async Task<Consultation?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Consultations
            .Include(c => c.Diagnoses)
            .Include(c => c.Notes)
            .Include(c => c.LabOrders)
            .Include(c => c.PrescriptionOrders)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task AddAsync(Consultation consultation, CancellationToken ct = default)
    {
        db.Consultations.Add(consultation);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Consultation consultation, CancellationToken ct = default)
    {
        db.Entry(consultation).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public async Task<ConsultationDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var c = await db.Consultations.AsNoTracking()
            .Include(x => x.Diagnoses)
            .Include(x => x.Notes)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (c is null) return null;

        return new ConsultationDetailDto(
            c.Id, c.PatientId, c.ClinicianUserId, c.Status.ToString(),
            c.StartedAtUtc, c.CompletedAtUtc,
            c.Triage is null ? null : new TriageDataDto(
                c.Triage.TemperatureCelsius, c.Triage.BloodPressure, c.Triage.PulseRate,
                c.Triage.RespiratoryRate, c.Triage.WeightKg),
            c.Diagnoses.Select(d => new DiagnosisDto(d.IcdCode, d.Description, d.IsPrimary)).ToArray(),
            c.Notes.Select(n => new ClinicalNoteDto(n.Content, n.AuthorUserId, n.RecordedAtUtc)).ToArray());
    }

    public async Task<IReadOnlyList<ConsultationSummaryDto>> GetByPatientAsync(Guid patientId, CancellationToken ct = default)
        => await db.Consultations.AsNoTracking()
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.StartedAtUtc)
            .Select(c => new ConsultationSummaryDto(
                c.Id, c.PatientId, c.ClinicianUserId, c.Status.ToString(),
                c.StartedAtUtc, c.CompletedAtUtc))
            .ToListAsync(ct);

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
