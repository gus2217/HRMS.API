using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Application.Abstractions;

public interface IQueueEntryRepository
{
    Task<QueueEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(QueueEntry entry, CancellationToken ct = default);
    Task UpdateAsync(QueueEntry entry, CancellationToken ct = default);

    /// <summary>Finds the queue entry linked to a consultation (for completion wiring).</summary>
    Task<QueueEntry?> GetByConsultationIdAsync(Guid consultationId, CancellationToken ct = default);

    Task<IReadOnlyList<QueueEntrySummaryDto>> SearchAsync(
        string? clinicType, string? status, int pageNumber, int pageSize, CancellationToken ct = default);

    Task<int> CountAsync(string? clinicType, string? status, CancellationToken ct = default);

    /// <summary>Next per-clinic daily sequence number (e.g. OPD-014 → 15).</summary>
    Task<int> NextSequenceAsync(Guid facilityId, string clinicType, DateOnly date, CancellationToken ct = default);
}
