using Jacana.SharedKernel.Domain;

namespace Jacana.Billing.Domain;

/// <summary>
/// A line on an invoice. Snapshots the service description and price at invoice time —
/// it never holds a hard FK to a deletable Service row, so historical invoices stay
/// correct even if a price-list item is later removed or renamed.
/// </summary>
public sealed class InvoiceLine : Entity<Guid>
{
    private InvoiceLine() { } // EF

    internal InvoiceLine(Guid id, string serviceCode, string description, int quantity, Money unitPrice)
        : base(id)
    {
        ServiceCode = serviceCode;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public string ServiceCode { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;

    public Money LineTotal => (UnitPrice * Quantity).Value;

    internal static Result<InvoiceLine> Create(string serviceCode, string description, int quantity, Money unitPrice)
    {
        if (string.IsNullOrWhiteSpace(serviceCode)) return Error.Validation("Service code is required.");
        if (string.IsNullOrWhiteSpace(description)) return Error.Validation("Service description is required.");
        if (quantity <= 0) return Error.Validation("Quantity must be positive.");
        return new InvoiceLine(Guid.NewGuid(), serviceCode.Trim(), description.Trim(), quantity, unitPrice);
    }
}
