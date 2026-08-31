namespace Jacana.Billing.Domain;

public enum InvoiceStatus { Draft, Issued, PartiallyPaid, Paid, Cancelled, WrittenOff }

/// <summary>
/// Per-line billing state. A line starts <see cref="Draft"/> (the service is
/// ordered but not yet delivered — med not dispensed, test not resulted) and is
/// marked <see cref="Charged"/> once delivery is confirmed.
/// </summary>
public enum InvoiceLineStatus { Draft, Charged }

public enum PaymentMethod { Cash, MPesa, ShaCover, BankTransfer, Insurance }

public enum PaymentStatus { Pending, Confirmed, Failed, Reversed }

public enum ShaClaimStatus { NotSubmitted, Submitted, UnderReview, Approved, Rejected, Paid }
