using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;

namespace Jacana.Clinical.Application.Abstractions;

public interface IAppointmentRequestRepository
{
    Task<AppointmentRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(AppointmentRequest request, CancellationToken ct = default);
    Task UpdateAsync(AppointmentRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<AppointmentRequestSummaryDto>> SearchAsync(
        string? clinicType, string? status, int pageNumber, int pageSize, CancellationToken ct = default);

    Task<int> CountAsync(string? clinicType, string? status, CancellationToken ct = default);
}
