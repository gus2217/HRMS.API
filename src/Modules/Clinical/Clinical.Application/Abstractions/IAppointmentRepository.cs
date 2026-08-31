using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;

namespace Jacana.Clinical.Application.Abstractions;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Appointment appointment, CancellationToken ct = default);
    Task UpdateAsync(Appointment appointment, CancellationToken ct = default);

    Task<Appointment?> GetByConsultationIdAsync(Guid consultationId, CancellationToken ct = default);

    Task<IReadOnlyList<AppointmentSummaryDto>> SearchAsync(
        string? clinicType, string? status, DateTime? fromUtc, DateTime? toUtc,
        int pageNumber, int pageSize, CancellationToken ct = default);

    Task<int> CountAsync(
        string? clinicType, string? status, DateTime? fromUtc, DateTime? toUtc,
        CancellationToken ct = default);

    /// <summary>Appointments for a month (calendar view), optionally clinic-filtered.</summary>
    Task<IReadOnlyList<AppointmentSummaryDto>> GetByMonthAsync(
        int year, int month, string? clinicType, CancellationToken ct = default);

    /// <summary>All appointments for a patient (medical record), newest first.</summary>
    Task<IReadOnlyList<AppointmentSummaryDto>> GetByPatientAsync(
        Guid patientId, CancellationToken ct = default);
}
