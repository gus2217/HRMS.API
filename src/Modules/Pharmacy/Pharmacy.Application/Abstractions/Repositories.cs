using Jacana.Pharmacy.Application.DTOs;
using Jacana.Pharmacy.Domain;

namespace Jacana.Pharmacy.Application.Abstractions;

public interface IPrescriptionRepository
{
    Task<Prescription?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Prescription prescription, CancellationToken ct = default);
    Task UpdateAsync(Prescription prescription, CancellationToken ct = default);
    Task<PrescriptionDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PrescriptionSummaryDto>> SearchAsync(
        string? status, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(string? status, CancellationToken ct = default);
}

public interface IDispenseRecordRepository
{
    Task AddAsync(DispenseRecord record, CancellationToken ct = default);
}
