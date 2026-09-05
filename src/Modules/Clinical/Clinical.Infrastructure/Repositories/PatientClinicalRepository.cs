using Jacana.Clinical.Application.Abstractions;
using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.Clinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Clinical.Infrastructure.Repositories;

public sealed class PatientClinicalRepository(ClinicalDbContext db) : IPatientClinicalRepository
{
    public Task AddVitalSignAsync(VitalSign vitalSign, CancellationToken ct = default)
    {
        db.VitalSigns.Add(vitalSign);
        return Task.CompletedTask;
    }

    public Task AddImmunizationAsync(Immunization immunization, CancellationToken ct = default)
    {
        db.Immunizations.Add(immunization);
        return Task.CompletedTask;
    }

    public Task AddConditionAsync(Condition condition, CancellationToken ct = default)
    {
        db.Conditions.Add(condition);
        return Task.CompletedTask;
    }

    public Task<Condition?> GetConditionAsync(Guid id, CancellationToken ct = default)
        => db.Conditions.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task UpdateConditionAsync(Condition condition, CancellationToken ct = default)
    {
        // Aggregate is already tracked from GetConditionAsync.
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<VitalSignDto>> GetVitalsAsync(Guid patientId, CancellationToken ct = default)
        => await db.VitalSigns.AsNoTracking()
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.RecordedAtUtc)
            .Select(v => new VitalSignDto(
                v.Id, v.PatientId, v.TemperatureCelsius, v.SystolicBp, v.DiastolicBp,
                v.PulseRate, v.RespiratoryRate, v.OxygenSaturation, v.WeightKg, v.HeightCm,
                v.Bmi, v.RecordedByUserId, v.RecordedAtUtc, null))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ImmunizationDto>> GetImmunizationsAsync(Guid patientId, CancellationToken ct = default)
        => await db.Immunizations.AsNoTracking()
            .Where(i => i.PatientId == patientId)
            .OrderByDescending(i => i.AdministeredDate)
            .Select(i => new ImmunizationDto(
                i.Id, i.PatientId, i.VaccineName, i.DoseNumber, i.AdministeredDate,
                i.NextDueDate, i.LotNumber, i.Site, i.Notes, i.RecordedByUserId, i.RecordedAtUtc, null))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ConditionDto>> GetConditionsAsync(Guid patientId, CancellationToken ct = default)
        => await db.Conditions.AsNoTracking()
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.Status == ConditionStatus.Active)
            .ThenByDescending(c => c.OnsetDate)
            .Select(c => new ConditionDto(
                c.Id, c.PatientId, c.Code, c.Description, c.Status.ToString(),
                c.OnsetDate, c.ResolvedDate, c.RecordedByUserId, c.RecordedAtUtc, null))
            .ToListAsync(ct);

    // ── Flags ────────────────────────────────────────────────────────────────

    public Task AddPatientFlagAsync(PatientFlag flag, CancellationToken ct = default)
    {
        db.PatientFlags.Add(flag);
        return Task.CompletedTask;
    }

    public Task<PatientFlag?> GetPatientFlagAsync(Guid id, CancellationToken ct = default)
        => db.PatientFlags.FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<IReadOnlyList<PatientFlagDto>> GetActiveFlagsAsync(Guid patientId, CancellationToken ct = default)
        => await db.PatientFlags.AsNoTracking()
            .Where(f => f.PatientId == patientId && f.IsActive)
            .OrderByDescending(f => f.CreatedAtUtc)
            .Select(f => new PatientFlagDto(
                f.Id, f.PatientId, f.Type.ToString(), f.Message, f.IsActive,
                f.CreatedByUserId, f.CreatedAtUtc, f.DeactivatedByUserId, f.DeactivatedAtUtc))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PatientFlagDto>> GetAllFlagsAsync(Guid patientId, CancellationToken ct = default)
        => await db.PatientFlags.AsNoTracking()
            .Where(f => f.PatientId == patientId)
            .OrderByDescending(f => f.CreatedAtUtc)
            .Select(f => new PatientFlagDto(
                f.Id, f.PatientId, f.Type.ToString(), f.Message, f.IsActive,
                f.CreatedByUserId, f.CreatedAtUtc, f.DeactivatedByUserId, f.DeactivatedAtUtc))
            .ToListAsync(ct);

    // ── Attachments ──────────────────────────────────────────────────────────

    public Task AddAttachmentAsync(PatientAttachment attachment, CancellationToken ct = default)
    {
        db.PatientAttachments.Add(attachment);
        return Task.CompletedTask;
    }

    public Task<PatientAttachment?> GetAttachmentAsync(Guid id, CancellationToken ct = default)
        => db.PatientAttachments.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<PatientAttachmentDto>> GetAttachmentsAsync(Guid patientId, CancellationToken ct = default)
        => await db.PatientAttachments.AsNoTracking()
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.UploadedAtUtc)
            .Select(a => new PatientAttachmentDto(
                a.Id, a.PatientId, a.FileName, a.ContentType, a.SizeBytes, a.Category,
                a.UploadedByUserId, a.UploadedAtUtc))
            .ToListAsync(ct);

    public Task DeleteAttachmentAsync(PatientAttachment attachment, CancellationToken ct = default)
    {
        db.PatientAttachments.Remove(attachment);
        return Task.CompletedTask;
    }

    // ── Diagnostic orders ────────────────────────────────────────────────────

    public Task AddDiagnosticOrderAsync(DiagnosticOrder order, CancellationToken ct = default)
    {
        db.DiagnosticOrders.Add(order);
        return Task.CompletedTask;
    }

    public Task<DiagnosticOrder?> GetDiagnosticOrderAsync(Guid id, CancellationToken ct = default)
        => db.DiagnosticOrders.FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task UpdateDiagnosticOrderAsync(DiagnosticOrder order, CancellationToken ct = default)
        => Task.CompletedTask; // already tracked from GetDiagnosticOrderAsync

    public async Task<IReadOnlyList<DiagnosticOrderDto>> GetDiagnosticOrdersByPatientAsync(Guid patientId, CancellationToken ct = default)
        => await db.DiagnosticOrders.AsNoTracking()
            .Where(o => o.PatientId == patientId)
            .OrderByDescending(o => o.OrderedAtUtc)
            .Select(o => MapOrder(o))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<DiagnosticOrderDto>> GetDiagnosticOrdersByConsultationAsync(Guid consultationId, CancellationToken ct = default)
        => await db.DiagnosticOrders.AsNoTracking()
            .Where(o => o.ConsultationId == consultationId)
            .OrderByDescending(o => o.OrderedAtUtc)
            .Select(o => MapOrder(o))
            .ToListAsync(ct);

    private static DiagnosticOrderDto MapOrder(DiagnosticOrder o) => new(
        o.Id, o.PatientId, o.ConsultationId, o.Type.ToString(), o.Name, o.BodySite,
        o.ClinicalIndication, o.Priority.ToString(), o.Status.ToString(),
        o.OrderedByUserId, o.OrderedAtUtc, o.Report, o.ReportedByUserId, o.ReportedAtUtc);
}
