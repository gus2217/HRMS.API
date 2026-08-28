using Jacana.Billing.Application.Abstractions;
using Jacana.Billing.Application.DTOs;
using Jacana.Billing.Domain;
using Jacana.Billing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Jacana.SharedKernel.Infrastructure.Persistence;

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
        // Aggregate already tracked from GetByIdAsync. New children carry
        // client-generated keys; EF DetectChanges would classify them as Modified
        // (phantom UPDATE, 0 rows). Mark them Added explicitly while still Detached.
        db.MarkNewChildrenAdded(invoice);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<InvoiceSummaryDto>> SearchAsync(
        string? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Invoices.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<InvoiceStatus>(status, true, out var parsed))
            query = query.Where(i => i.Status == parsed);

        return await query
            .OrderByDescending(i => i.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InvoiceSummaryDto(
                i.Id, i.PatientId, i.Status.ToString(),
                i.Lines.Sum(l => l.UnitPrice.Amount * l.Quantity), i.CreatedAtUtc))
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? status, CancellationToken ct = default)
    {
        var query = db.Invoices.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<InvoiceStatus>(status, true, out var parsed))
            query = query.Where(i => i.Status == parsed);
        return query.CountAsync(ct);
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
        // Entity already tracked from GetByIdAsync; mutations auto-detected.
        // Never force graph state — it marks new children as Modified (UPDATE 0 rows).
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
        // Entity already tracked from GetByIdAsync; mutations auto-detected.
        // Never force graph state — it marks new children as Modified (UPDATE 0 rows).
        return Task.CompletedTask;
    }

    public Task<ShaClaim?> GetByReferenceAsync(string reference, CancellationToken ct = default)
        => db.ShaClaims.FirstOrDefaultAsync(c => c.ShaClaimReference == reference, ct);
}
