using Jacana.Clinical.Application.Abstractions;
using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.Clinical.Infrastructure.Persistence;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Clinical.Infrastructure.Repositories;

public sealed class AppointmentRepository(ClinicalDbContext db) : IAppointmentRepository
{
    public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Appointments.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Appointment?> GetByConsultationIdAsync(Guid consultationId, CancellationToken ct = default)
        => db.Appointments.FirstOrDefaultAsync(a => a.ConsultationId == consultationId, ct);

    public Task AddAsync(Appointment appointment, CancellationToken ct = default)
    {
        db.Appointments.Add(appointment);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Appointment appointment, CancellationToken ct = default)
    {
        db.MarkNewChildrenAdded(appointment);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<AppointmentSummaryDto>> SearchAsync(
        string? clinicType, string? status, DateTime? fromUtc, DateTime? toUtc,
        int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Appointments.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(clinicType))
            query = query.Where(a => a.ClinicType == clinicType);
        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<AppointmentStatus>(status, true, out var parsed))
            query = query.Where(a => a.Status == parsed);
        if (fromUtc is not null) query = query.Where(a => a.ScheduledAtUtc >= fromUtc);
        if (toUtc is not null) query = query.Where(a => a.ScheduledAtUtc < toUtc);

        return await query
            .OrderBy(a => a.ScheduledAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => Map(a))
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(
        string? clinicType, string? status, DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default)
    {
        var query = db.Appointments.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(clinicType))
            query = query.Where(a => a.ClinicType == clinicType);
        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<AppointmentStatus>(status, true, out var parsed))
            query = query.Where(a => a.Status == parsed);
        if (fromUtc is not null) query = query.Where(a => a.ScheduledAtUtc >= fromUtc);
        if (toUtc is not null) query = query.Where(a => a.ScheduledAtUtc < toUtc);
        return query.CountAsync(ct);
    }

    public async Task<IReadOnlyList<AppointmentSummaryDto>> GetByMonthAsync(
        int year, int month, string? clinicType, CancellationToken ct = default)
    {
        var fromUtc = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = fromUtc.AddMonths(1);

        var query = db.Appointments.AsNoTracking()
            .Where(a => a.ScheduledAtUtc >= fromUtc && a.ScheduledAtUtc < toUtc);
        if (!string.IsNullOrWhiteSpace(clinicType))
            query = query.Where(a => a.ClinicType == clinicType);

        return await query
            .OrderBy(a => a.ScheduledAtUtc)
            .Select(a => Map(a))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AppointmentSummaryDto>> GetByPatientAsync(
        Guid patientId, CancellationToken ct = default)
    {
        return await db.Appointments.AsNoTracking()
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.ScheduledAtUtc)
            .Select(a => Map(a))
            .ToListAsync(ct);
    }

    public Task<bool> HasOverlapAsync(
        string clinicType, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
        => db.Appointments.AsNoTracking()
            .AnyAsync(a =>
                a.ClinicType == clinicType
                && (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.InProgress)
                && a.ScheduledAtUtc < toUtc
                && a.ScheduledAtUtc.AddMinutes(a.DurationMinutes) > fromUtc, ct);

    private static AppointmentSummaryDto Map(Appointment a) => new(
        a.Id, a.PatientId, a.ClinicType, a.Type.ToString(), a.Status.ToString(),
        a.ScheduledAtUtc, a.DurationMinutes, a.Reason, a.RecurrenceGroupId,
        a.RecurrencePattern.ToString(), a.CreatedByUserId, a.CreatedAtUtc,
        a.ConsultationId, a.StartedAtUtc, a.CompletedAtUtc);
}

public sealed class AppointmentRequestRepository(ClinicalDbContext db) : IAppointmentRequestRepository
{
    public Task<AppointmentRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.AppointmentRequests.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task AddAsync(AppointmentRequest request, CancellationToken ct = default)
    {
        db.AppointmentRequests.Add(request);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AppointmentRequest request, CancellationToken ct = default)
    {
        db.MarkNewChildrenAdded(request);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<AppointmentRequestSummaryDto>> SearchAsync(
        string? clinicType, string? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.AppointmentRequests.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(clinicType))
            query = query.Where(r => r.ClinicType == clinicType);
        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<AppointmentRequestStatus>(status, true, out var parsed))
            query = query.Where(r => r.Status == parsed);

        return await query
            .OrderByDescending(r => r.RequestedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new AppointmentRequestSummaryDto(
                r.Id, r.PatientId, r.ClinicType, r.Reason, r.Notes, r.PreferredDate,
                r.Status.ToString(), r.RequestedByUserId, r.RequestedAtUtc,
                r.ApprovedByUserId, r.ApprovedAtUtc, r.AppointmentId))
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? clinicType, string? status, CancellationToken ct = default)
    {
        var query = db.AppointmentRequests.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(clinicType))
            query = query.Where(r => r.ClinicType == clinicType);
        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<AppointmentRequestStatus>(status, true, out var parsed))
            query = query.Where(r => r.Status == parsed);
        return query.CountAsync(ct);
    }
}
