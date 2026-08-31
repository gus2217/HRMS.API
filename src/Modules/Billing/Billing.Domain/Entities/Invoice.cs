using Jacana.SharedKernel.Domain;

namespace Jacana.Billing.Domain;

/// <summary>
/// An invoice. <see cref="TotalAmount"/> is a computed property (sum of lines) that is
/// also persisted as a shadow column for query performance and reconciled on mutation.
/// </summary>
public sealed class Invoice : AggregateRoot<Guid>
{
    private readonly List<InvoiceLine> _lines = new();

    private Invoice() { } // EF

    private Invoice(Guid id, FacilityId facilityId, Guid patientId, Guid? consultationId)
        : base(id)
    {
        FacilityId = facilityId;
        PatientId = patientId;
        ConsultationId = consultationId;
        Status = InvoiceStatus.Draft;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public Guid? ConsultationId { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public PaymentMethod? PrimaryPaymentMethod { get; private set; }

    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();

    /// <summary>Computed total (sum of lines). Not persisted — the reporting layer
    /// recomputes from invoice_lines in SQL.</summary>
    public Money TotalAmount
    {
        get
        {
            var total = Money.Zero();
            foreach (var line in _lines)
                total = (total + line.LineTotal).Value;
            return total;
        }
    }

    public static Result<Invoice> Create(Guid id, FacilityId facilityId, Guid patientId, Guid? consultationId)
    {
        if (patientId == Guid.Empty) return Error.Validation("Patient is required.");
        return new Invoice(id, facilityId, patientId, consultationId);
    }

    public Result AddLine(string serviceCode, string description, int quantity, Money unitPrice)
        => AddLine(serviceCode, description, quantity, unitPrice, string.Empty, null);

    public Result AddLine(
        string serviceCode, string description, int quantity, Money unitPrice,
        string sourceType, Guid? sourceReferenceId)
    {
        if (Status != InvoiceStatus.Draft)
            return Error.InvalidOperation("Cannot add lines to a non-draft invoice.");

        var line = InvoiceLine.Create(serviceCode, description, quantity, unitPrice, sourceType, sourceReferenceId);
        if (line.IsFailure) return line.Error;
        _lines.Add(line.Value);
        return Result.Success();
    }

    /// <summary>Marks every line originating from <paramref name="sourceReferenceId"/> as charged.</summary>
    public void ChargeLines(Guid sourceReferenceId)
    {
        foreach (var line in _lines)
        {
            if (line.SourceReferenceId == sourceReferenceId && line.Status == InvoiceLineStatus.Draft)
                line.MarkCharged();
        }
    }

    /// <summary>Marks all draft lines charged (the whole visit was delivered).</summary>
    public void ChargeAllLines()
    {
        foreach (var line in _lines)
        {
            if (line.Status == InvoiceLineStatus.Draft)
                line.MarkCharged();
        }
    }

    public Result Issue(PaymentMethod? primaryMethod = null)
    {
        if (Status != InvoiceStatus.Draft)
            return Error.InvalidOperation("Only draft invoices can be issued.");
        if (_lines.Count == 0)
            return Error.InvalidOperation("Cannot issue an invoice with no lines.");

        Status = InvoiceStatus.Issued;
        PrimaryPaymentMethod = primaryMethod;
        return Result.Success();
    }

    public Result RecordPayment(Money amountPaid)
    {
        if (Status is not (InvoiceStatus.Issued or InvoiceStatus.PartiallyPaid))
            return Error.InvalidOperation($"Cannot record payment on a {Status} invoice.");

        var total = TotalAmount;
        if (amountPaid.Amount > total.Amount)
            return Error.InvalidOperation("Payment exceeds the invoice total.");

        Status = amountPaid.Amount >= total.Amount
            ? InvoiceStatus.Paid
            : InvoiceStatus.PartiallyPaid;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
            return Error.InvalidOperation($"Cannot cancel a {Status} invoice.");
        Status = InvoiceStatus.Cancelled;
        return Result.Success();
    }
}
