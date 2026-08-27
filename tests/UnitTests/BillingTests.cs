using Jacana.Billing.Domain;
using Jacana.SharedKernel.Domain;
using Xunit;

namespace Jacana.Tests.Unit.Billing;

public class InvoiceTests
{
    private static Invoice NewInvoice()
        => Invoice.Create(Guid.NewGuid(), FacilityId.New(), Guid.NewGuid(), null).Value;

    [Fact]
    public void TotalAmount_sums_lines()
    {
        var invoice = NewInvoice();
        invoice.AddLine("CONSULT", "Consultation", 1, Money.Create(500m).Value);
        invoice.AddLine("LAB-CBC", "Complete Blood Count", 2, Money.Create(250m).Value);

        Assert.Equal(1000m, invoice.TotalAmount.Amount);
    }

    [Fact]
    public void Cannot_issue_empty_invoice()
    {
        var invoice = NewInvoice();
        Assert.True(invoice.Issue().IsFailure);
    }

    [Fact]
    public void Cannot_add_lines_to_issued_invoice()
    {
        var invoice = NewInvoice();
        invoice.AddLine("CONSULT", "Consultation", 1, Money.Create(500m).Value);
        invoice.Issue();

        Assert.True(invoice.AddLine("LAB", "Lab", 1, Money.Create(100m).Value).IsFailure);
    }

    [Fact]
    public void Full_payment_marks_paid()
    {
        var invoice = NewInvoice();
        invoice.AddLine("CONSULT", "Consultation", 1, Money.Create(500m).Value);
        invoice.Issue();

        var result = invoice.RecordPayment(Money.Create(500m).Value);
        Assert.True(result.IsSuccess);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    }

    [Fact]
    public void Overpayment_rejected()
    {
        var invoice = NewInvoice();
        invoice.AddLine("CONSULT", "Consultation", 1, Money.Create(500m).Value);
        invoice.Issue();

        Assert.True(invoice.RecordPayment(Money.Create(600m).Value).IsFailure);
    }
}

public class PaymentTests
{
    [Fact]
    public void Requires_positive_amount_and_reference()
    {
        var facility = FacilityId.New();
        Assert.True(Payment.Create(Guid.NewGuid(), facility, Guid.NewGuid(), Money.Create(0m).Value,
            PaymentMethod.Cash, "ref-1", PaymentStatus.Confirmed, DateTime.UtcNow).IsFailure);
        Assert.True(Payment.Create(Guid.NewGuid(), facility, Guid.NewGuid(), Money.Create(10m).Value,
            PaymentMethod.Cash, "", PaymentStatus.Confirmed, DateTime.UtcNow).IsFailure);
    }
}

public class ShaClaimTests
{
    [Fact]
    public void Submit_requires_invoice_and_reference()
    {
        Assert.True(ShaClaim.Submit(Guid.NewGuid(), FacilityId.New(), Guid.Empty, "REF-1", DateTime.UtcNow).IsFailure);
        Assert.True(ShaClaim.Submit(Guid.NewGuid(), FacilityId.New(), Guid.NewGuid(), "", DateTime.UtcNow).IsFailure);
    }

    [Fact]
    public void Cannot_reject_paid_claim()
    {
        var claim = ShaClaim.Submit(Guid.NewGuid(), FacilityId.New(), Guid.NewGuid(), "REF-1", DateTime.UtcNow).Value;
        claim.MarkPaid();
        Assert.True(claim.Reject("too late").IsFailure);
    }
}
