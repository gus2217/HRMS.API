using Jacana.Laboratory.Application.DTOs;
using Jacana.Laboratory.Domain;

namespace Jacana.Laboratory.Application.Abstractions;

public interface ILabOrderRepository
{
    Task<LabOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(LabOrder order, CancellationToken ct = default);
    Task UpdateAsync(LabOrder order, CancellationToken ct = default);
    Task<LabOrderDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<LabOrderSummaryDto>> SearchAsync(
        string? status, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(string? status, CancellationToken ct = default);
}
