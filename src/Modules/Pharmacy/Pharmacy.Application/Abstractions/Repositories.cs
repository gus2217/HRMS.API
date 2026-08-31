using Jacana.Pharmacy.Application.DTOs;
using Jacana.Pharmacy.Domain;

namespace Jacana.Pharmacy.Application.Abstractions;

public interface IPrescriptionRepository
{
    Task<Prescription?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Prescription prescription, CancellationToken ct = default);
    Task UpdateAsync(Prescription prescription, CancellationToken ct = default);
    Task<PrescriptionDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PrescriptionDetailDto>> GetByConsultationAsync(Guid consultationId, CancellationToken ct = default);
    Task<IReadOnlyList<PrescriptionSummaryDto>> SearchAsync(
        string? status, Guid? patientId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(string? status, Guid? patientId, CancellationToken ct = default);

    /// <summary>
    /// Quantity of a drug already committed to active (un-dispensed) prescription
    /// items. Prescribers must not over-commit beyond physical stock minus this.
    /// </summary>
    Task<int> GetReservedQuantityAsync(Guid drugId, CancellationToken ct = default);

    /// <summary>Reserved quantity per drug (all active prescriptions, batch).</summary>
    Task<IReadOnlyDictionary<Guid, int>> GetReservedQuantitiesAsync(CancellationToken ct = default);
}

public interface IDispenseRecordRepository
{
    Task AddAsync(DispenseRecord record, CancellationToken ct = default);
}
