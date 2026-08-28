using Jacana.Billing.Application.DTOs;
using Jacana.Billing.Domain;

namespace Jacana.Billing.Application.Abstractions;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Invoice?> GetByConsultationAsync(Guid consultationId, CancellationToken ct = default);
    Task AddAsync(Invoice invoice, CancellationToken ct = default);
    Task UpdateAsync(Invoice invoice, CancellationToken ct = default);
    Task<InvoiceDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<InvoiceSummaryDto>> SearchAsync(
        string? status, Guid? consultationId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(string? status, Guid? consultationId, CancellationToken ct = default);

    /// <summary>
    /// Detaches the aggregate and its lines from the change tracker. Auto-billing
    /// handlers run in one outbox batch sharing a scoped DbContext; without this,
    /// the next handler re-uses a tracked entity with a stale RowVersion and the
    /// UPDATE affects 0 rows (optimistic concurrency failure).
    /// </summary>
    void Detach(Invoice invoice);
}

public interface IPaymentRepository
{
    Task<Payment?> GetByProviderReferenceAsync(string providerTransactionReference, CancellationToken ct = default);
    Task AddAsync(Payment payment, CancellationToken ct = default);
    Task UpdateAsync(Payment payment, CancellationToken ct = default);
}

public interface IShaClaimRepository
{
    Task AddAsync(ShaClaim claim, CancellationToken ct = default);
    Task UpdateAsync(ShaClaim claim, CancellationToken ct = default);
    Task<ShaClaim?> GetByReferenceAsync(string reference, CancellationToken ct = default);
}
