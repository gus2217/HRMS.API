using Jacana.Billing.Application.Abstractions;
using Jacana.Billing.Application.DTOs;
using Jacana.Billing.Domain;
using Jacana.Billing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Billing.Infrastructure.Repositories;

public sealed class InvoiceRepository(BillingDbContext db) : IInvoiceRepository
{
    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Invoices.Include(i => i.Lines).FirstOrDefaultAsync(i => i.Id == id, ct);

    public Task AddAsync(Invoice invoice, CancellationToken ct = default)
    {
        db.Invoices.Add(invoice);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Invoice invoice, CancellationToken ct = default)
    {
        db.Invoices.Update(invoice);
        return Task.CompletedTask;
    }

    public async Task<InvoiceDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var i = await db.Invoices.AsNoTracking()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (i is null) return null;

        return new InvoiceDetailDto(
            i.Id, i.PatientId, i.ConsultationId, i.Status.ToString(), i.TotalAmount.Amount,
            i.PrimaryPaymentMethod?.ToString(),
            i.Lines.Select(l => new InvoiceLineDto(
                l.Id, l.ServiceCode, l.Description, l.Quantity,
                l.UnitPrice.Amount, l.LineTotal.Amount)).ToArray());
    }
}

public sealed class PaymentRepository(BillingDbContext db) : IPaymentRepository
{
    public Task<Payment?> GetByProviderReferenceAsync(string providerTransactionReference, CancellationToken ct = default)
        => db.Payments.FirstOrDefaultAsync(p => p.ProviderTransactionReference == providerTransactionReference, ct);

    public Task AddAsync(Payment payment, CancellationToken ct = default)
    {
        db.Payments.Add(payment);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        db.Payments.Update(payment);
        return Task.CompletedTask;
    }
}

public sealed class ShaClaimRepository(BillingDbContext db) : IShaClaimRepository
{
    public Task AddAsync(ShaClaim claim, CancellationToken ct = default)
    {
        db.ShaClaims.Add(claim);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ShaClaim claim, CancellationToken ct = default)
    {
        db.ShaClaims.Update(claim);
        return Task.CompletedTask;
    }

    public Task<ShaClaim?> GetByReferenceAsync(string reference, CancellationToken ct = default)
        => db.ShaClaims.FirstOrDefaultAsync(c => c.ShaClaimReference == reference, ct);
}
