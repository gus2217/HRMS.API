using Jacana.Inpatient.Application.Abstractions;
using Jacana.Inpatient.Application.DTOs;
using Jacana.Inpatient.Domain;
using Jacana.Inpatient.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Jacana.SharedKernel.Infrastructure.Persistence;

namespace Jacana.Inpatient.Infrastructure.Repositories;

public sealed class AdmissionRepository(InpatientDbContext db) : IAdmissionRepository
{
    public Task<Admission?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Admissions
            .Include(a => a.Notes)
            .Include(a => a.MedicalRecords).ThenInclude(r => r.Attachments)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<WardRecordAttachment?> GetAttachmentAsync(Guid attachmentId, CancellationToken ct = default)
        => db.Set<WardRecordAttachment>().FirstOrDefaultAsync(a => a.Id == attachmentId, ct);

    public Task AddAsync(Admission admission, CancellationToken ct = default)
    {
        db.Admissions.Add(admission);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Admission admission, CancellationToken ct = default)
    {
        // Aggregate already tracked from GetByIdAsync. New children carry
        // client-generated keys; EF DetectChanges would classify them as Modified
        // (phantom UPDATE, 0 rows). Mark them Added explicitly while still
        // Detached. Auto-detect must stay OFF across the whole walk — calling
        // db.Entry(...) with auto-detect enabled re-runs DetectChanges and would
        // reclassify a just-added grandchild (record attachment) as Modified
        // before its Detached→Added check runs.
        var autoDetect = db.ChangeTracker.AutoDetectChangesEnabled;
        db.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            db.MarkNewChildrenAdded(admission);

            // Medical records + their attachments are 1:N reference navigations (not
            // enumerable props on the aggregate itself) — walk them explicitly.
            foreach (var record in admission.MedicalRecords)
            {
                var entry = db.Entry(record);
                if (entry.State == EntityState.Detached)
                    entry.State = EntityState.Added;
                foreach (var attachment in record.Attachments)
                {
                    var aEntry = db.Entry(attachment);
                    if (aEntry.State == EntityState.Detached)
                        aEntry.State = EntityState.Added;
                }
            }
        }
        finally
        {
            db.ChangeTracker.AutoDetectChangesEnabled = autoDetect;
        }

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<AdmissionSummaryDto>> SearchAsync(
        bool activeOnly, Guid? patientId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Admissions.AsNoTracking();
        if (activeOnly)
            query = query.Where(a => a.Status != AdmissionStatus.Discharged);
        if (patientId.HasValue)
            query = query.Where(a => a.PatientId == patientId.Value);

        return await query
            .OrderByDescending(a => a.AdmittedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AdmissionSummaryDto(
                a.Id, a.PatientId, a.WardId, a.WardName, a.BedNumber,
                a.Status.ToString(), a.AdmittedAtUtc))
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(bool activeOnly, Guid? patientId, CancellationToken ct = default)
    {
        var query = db.Admissions.AsNoTracking();
        if (activeOnly)
            query = query.Where(a => a.Status != AdmissionStatus.Discharged);
        if (patientId.HasValue)
            query = query.Where(a => a.PatientId == patientId.Value);
        return query.CountAsync(ct);
    }

    public async Task<AdmissionDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var a = await db.Admissions.AsNoTracking()
            .Include(x => x.Notes)
            .Include(x => x.MedicalRecords).ThenInclude(r => r.Attachments)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (a is null) return null;

        return new AdmissionDetailDto(
            a.Id, a.PatientId, a.AdmittingClinicianUserId, a.WardId, a.WardName, a.BedNumber,
            a.AdmittingDiagnosis, a.AttendingClinicianUserId,
            a.Status.ToString(), a.AdmittedAtUtc, a.DischargedAtUtc,
            a.Notes.Select(n => new WardNoteDto(n.Content, n.AuthorUserId, n.RecordedAtUtc)).ToArray(),
            a.MedicalRecords.Select(r => new WardMedicalRecordDto(
                r.Id, r.RecordedByUserId, r.RecordedAtUtc,
                r.TemperatureCelsius, r.SystolicBp, r.DiastolicBp, r.PulseRate,
                r.RespiratoryRate, r.OxygenSaturation, r.WeightKg,
                r.Subjective, r.Objective, r.Assessment, r.Plan, r.IsComplete,
                r.Attachments.Select(at => new WardRecordAttachmentDto(
                    at.Id, at.FileName, at.ContentType, at.SizeBytes, at.UploadedByUserId, at.UploadedAtUtc)).ToArray())).ToArray(),
            a.HasCompleteMedicalRecord,
            AdmittingClinicianName: null,
            AttendingClinicianName: null);
    }

    public Task<int> GetOccupiedBedCountAsync(Guid wardId, CancellationToken ct = default)
        => db.Admissions.AsNoTracking()
            .CountAsync(a => a.WardId == wardId && a.Status != AdmissionStatus.Discharged, ct);

    public async Task<IReadOnlyList<WardOccupancyDto>> GetWardOccupancyAsync(CancellationToken ct = default)
    {
        var grouped = await db.Admissions.AsNoTracking()
            .Where(a => a.Status != AdmissionStatus.Discharged)
            .GroupBy(a => new { a.WardId, a.WardName })
            .Select(g => new { g.Key.WardId, g.Key.WardName, Occupied = g.Count() })
            .ToListAsync(ct);

        var wards = await db.Wards.AsNoTracking()
            .Select(w => new { w.Id, w.Name, w.TotalBeds })
            .ToListAsync(ct);

        return wards.Select(w =>
        {
            var match = grouped.FirstOrDefault(g => g.WardId == w.Id);
            return new WardOccupancyDto(w.Id, w.Name, match?.Occupied ?? 0, w.TotalBeds);
        }).ToArray();
    }
}
