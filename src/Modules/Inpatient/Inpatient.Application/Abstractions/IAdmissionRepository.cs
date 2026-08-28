using Jacana.Inpatient.Application.DTOs;
using Jacana.Inpatient.Domain;

namespace Jacana.Inpatient.Application.Abstractions;

public interface IAdmissionRepository
{
    Task<Admission?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Admission admission, CancellationToken ct = default);
    Task UpdateAsync(Admission admission, CancellationToken ct = default);
    Task<AdmissionDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<WardOccupancyDto>> GetWardOccupancyAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AdmissionSummaryDto>> SearchAsync(
        bool activeOnly, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(bool activeOnly, CancellationToken ct = default);
}
