namespace Jacana.Billing.Domain;

public enum InvoiceStatus { Draft, Issued, PartiallyPaid, Paid, Cancelled, WrittenOff }

public enum PaymentMethod { Cash, MPesa, ShaCover, BankTransfer, Insurance }

public enum PaymentStatus { Pending, Confirmed, Failed, Reversed }

public enum ShaClaimStatus { NotSubmitted, Submitted, UnderReview, Approved, Rejected, Paid }
