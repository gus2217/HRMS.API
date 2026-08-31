using Jacana.Clinical.Domain;
using Jacana.Inventory.Domain;
using Jacana.Laboratory.Domain;
using Jacana.Pharmacy.Domain;
using Jacana.Billing.Application.Abstractions;
using Jacana.Billing.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Options;

namespace Jacana.Billing.Application.Features.Billing.DomainEventHandlers;

/// <summary>Fees used when auto-billing consultations (config section "Billing").</summary>
public sealed class BillingFeeOptions
{
    public decimal ConsultationFee { get; set; } = 500m;
    public decimal DefaultTestFee { get; set; } = 300m;
}

/// <summary>
/// Consumes clinical events via the outbox and accumulates charges on a single
/// draft invoice per consultation. The consultation-completed handler adds the
/// consultation fee and issues the invoice so cashiers/receptionists see it.
/// Cross-module coupling is one-directional via domain events — never a direct call.
/// </summary>
public sealed class PrescriptionCreatedBillingHandler(
    IInvoiceRepository invoices,
    IInventoryPricingQuery pricing,
    IEnumerable<IUnitOfWork> unitsOfWork)
    : INotificationHandler<DomainEventNotification<PrescriptionCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PrescriptionCreatedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;

        var invoice = await AutoBilling.FindOrCreateDraftAsync(
            invoices, e.FacilityId, e.PatientId, e.ConsultationId, ct);
        if (invoice is null) return;

        foreach (var item in e.Items)
        {
            var price = await pricing.GetPriceAsync(item.DrugId, ct);
            if (price is null) continue; // drug no longer in catalog — skip line

            var unitPrice = Money.Create(price.UnitPrice);
            if (unitPrice.IsFailure) continue;

            var add = invoice.AddLine(
                price.Code, $"{price.Name} ({price.Category}) — {item.DosageInstructions}",
                item.QuantityPrescribed, unitPrice.Value, "Prescription", e.PrescriptionId);
            if (add.IsFailure) continue;
        }

        if (invoice.Lines.Count > 0)
        {
            await invoices.UpdateAsync(invoice, ct); // marks client-keyed new lines Added (phantom-UPDATE fix)
            await AutoBilling.CommitAsync(unitsOfWork, ct);
            invoices.Detach(invoice);
        }
    }
}

public sealed class LabOrderCreatedBillingHandler(
    IInvoiceRepository invoices,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IOptions<BillingFeeOptions> options)
    : INotificationHandler<DomainEventNotification<LabOrderCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<LabOrderCreatedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;

        var invoice = await AutoBilling.FindOrCreateDraftAsync(
            invoices, e.FacilityId, e.PatientId, e.ConsultationId, ct);
        if (invoice is null) return;

        var fee = Money.Create(options.Value.DefaultTestFee);
        if (fee.IsFailure) return;

        foreach (var test in e.Tests)
        {
            var add = invoice.AddLine(test.TestCode, test.TestName, 1, fee.Value, "Lab", e.LabOrderId);
            if (add.IsFailure) continue;
        }

        if (invoice.Lines.Count > 0)
        {
            await invoices.UpdateAsync(invoice, ct); // marks client-keyed new lines Added (phantom-UPDATE fix)
            await AutoBilling.CommitAsync(unitsOfWork, ct);
            invoices.Detach(invoice);
        }
    }
}

public sealed class ConsultationCompletedBillingHandler(
    IInvoiceRepository invoices,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IOptions<BillingFeeOptions> options)
    : INotificationHandler<DomainEventNotification<ConsultationCompletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ConsultationCompletedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;

        var invoice = await AutoBilling.FindOrCreateDraftAsync(
            invoices, e.FacilityId, e.PatientId, e.ConsultationId, ct);
        if (invoice is null) return;

        var fee = Money.Create(options.Value.ConsultationFee);
        if (fee.IsFailure) return;

        // Idempotent: the fee is already on the draft (added at consultation start);
        // only add it here as a fallback for consultations started before that hook.
        AutoBilling.AddLineIfMissing(invoice, "CONSULT", "Consultation fee", 1, fee.Value, "Consultation", e.ConsultationId);

        // Charge the consultation fee + any still-draft lines, then issue the bill.
        invoice.ChargeLines(e.ConsultationId);
        invoice.Issue();
        await invoices.UpdateAsync(invoice, ct); // marks client-keyed new lines Added (phantom-UPDATE fix)
        await AutoBilling.CommitAsync(unitsOfWork, ct);
        invoices.Detach(invoice);
    }
}

/// <summary>
/// Opens the patient's draft invoice the moment the consultation enters clinical
/// work — the doctor sees a running bill immediately, not only at completion.
/// </summary>
public sealed class ConsultationStartedBillingHandler(
    IInvoiceRepository invoices,
    IEnumerable<IUnitOfWork> unitsOfWork,
    IOptions<BillingFeeOptions> options)
    : INotificationHandler<DomainEventNotification<ConsultationStartedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<ConsultationStartedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;

        var invoice = await AutoBilling.FindOrCreateDraftAsync(
            invoices, e.FacilityId, e.PatientId, e.ConsultationId, ct);
        if (invoice is null) return;

        var fee = Money.Create(options.Value.ConsultationFee);
        if (fee.IsFailure) return;

        AutoBilling.AddLineIfMissing(invoice, "CONSULT", "Consultation fee", 1, fee.Value, "Consultation", e.ConsultationId);

        await invoices.UpdateAsync(invoice, ct);
        await AutoBilling.CommitAsync(unitsOfWork, ct);
        invoices.Detach(invoice);
    }
}

/// <summary>
/// Charges a prescription's draft lines the moment it is fully dispensed.
/// </summary>
public sealed class PrescriptionFullyDispensedBillingHandler(
    IInvoiceRepository invoices,
    IEnumerable<IUnitOfWork> unitsOfWork)
    : INotificationHandler<DomainEventNotification<PrescriptionFullyDispensedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<PrescriptionFullyDispensedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var invoice = await invoices.GetByConsultationAsync(e.ConsultationId, ct);
        if (invoice is null) return;

        invoice.ChargeLines(e.PrescriptionId);
        await invoices.UpdateAsync(invoice, ct);
        await AutoBilling.CommitAsync(unitsOfWork, ct);
        invoices.Detach(invoice);
    }
}

/// <summary>
/// Charges a lab order's draft lines the moment it is completed (all tests resulted).
/// </summary>
public sealed class LabOrderCompletedBillingHandler(
    IInvoiceRepository invoices,
    IEnumerable<IUnitOfWork> unitsOfWork)
    : INotificationHandler<DomainEventNotification<LabOrderCompletedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<LabOrderCompletedDomainEvent> notification, CancellationToken ct)
    {
        var e = notification.DomainEvent;
        var invoice = await invoices.GetByConsultationAsync(e.ConsultationId, ct);
        if (invoice is null) return;

        invoice.ChargeLines(e.LabOrderId);
        await invoices.UpdateAsync(invoice, ct);
        await AutoBilling.CommitAsync(unitsOfWork, ct);
        invoices.Detach(invoice);
    }
}

internal static class AutoBilling
{
    /// <summary>
    /// Returns the consultation's draft invoice, creating one when none exists.
    /// If a previous invoice was already issued (post-completion charges), a fresh
    /// draft is created rather than mutating a settled bill.
    /// </summary>
    public static async Task<Invoice?> FindOrCreateDraftAsync(
        IInvoiceRepository invoices,
        Guid facilityId,
        Guid patientId,
        Guid consultationId,
        CancellationToken ct)
    {
        var existing = await invoices.GetByConsultationAsync(consultationId, ct);
        if (existing is { Status: InvoiceStatus.Draft })
            return existing;

        var created = Invoice.Create(Guid.NewGuid(), FacilityId.From(facilityId), patientId, consultationId);
        if (created.IsFailure) return null;

        await invoices.AddAsync(created.Value, ct);
        return created.Value;
    }

    /// <summary>
    /// Adds a line only if no line with the same service code already exists.
    /// Keeps the auto-billing handlers idempotent across a consultation's lifetime.
    /// </summary>
    public static void AddLineIfMissing(
        Invoice invoice, string serviceCode, string description, int quantity, Money unitPrice,
        string sourceType, Guid? sourceReferenceId)
    {
        if (invoice.Lines.Any(l => l.ServiceCode == serviceCode))
            return;
        invoice.AddLine(serviceCode, description, quantity, unitPrice, sourceType, sourceReferenceId);
    }

    /// <summary>
    /// Commits every unit of work with tracked changes. Mirrors TransactionBehavior:
    /// each module registers its own IUnitOfWork, so a plain single-IUnitOfWork
    /// injection would resolve the last-registered context and silently drop the
    /// invoice write. This commits the Billing DbContext (the one that changed).
    /// </summary>
    public static Task CommitAsync(IEnumerable<IUnitOfWork> unitsOfWork, CancellationToken ct)
    {
        var tasks = unitsOfWork
            .Where(u => u.HasChanges)
            .Select(u => u.SaveChangesAsync(ct));
        return Task.WhenAll(tasks);
    }
}
