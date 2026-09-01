using Jacana.Inpatient.Application.DTOs;
using Jacana.Inpatient.Domain;

namespace Jacana.Inpatient.Application.Abstractions;

public interface IWardRepository
{
    Task<Ward?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Ward ward, CancellationToken ct = default);
    Task UpdateAsync(Ward ward, CancellationToken ct = default);
    Task<IReadOnlyList<WardDto>> ListAsync(bool activeOnly, CancellationToken ct = default);
}
