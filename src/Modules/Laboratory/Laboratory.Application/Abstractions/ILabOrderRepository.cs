using Jacana.Laboratory.Application.DTOs;
using Jacana.Laboratory.Domain;

namespace Jacana.Laboratory.Application.Abstractions;

public interface ILabOrderRepository
{
    Task<LabOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(LabOrder order, CancellationToken ct = default);
    Task UpdateAsync(LabOrder order, CancellationToken ct = default);
    Task<LabOrderDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);
}
