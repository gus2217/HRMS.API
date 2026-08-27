using Jacana.Billing.Application.DTOs;
using Jacana.Billing.Domain;

namespace Jacana.Billing.Application.Abstractions;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Invoice invoice, CancellationToken ct = default);
    Task UpdateAsync(Invoice invoice, CancellationToken ct = default);
    Task<InvoiceDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);
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
