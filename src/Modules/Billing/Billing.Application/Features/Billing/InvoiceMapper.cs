using Jacana.Billing.Application.DTOs;
using Jacana.Billing.Domain;

namespace Jacana.Billing.Application.Features.Billing;

/// <summary>
/// Maps an in-memory <see cref="Invoice"/> aggregate to its detail DTO.
/// Handlers use this after mutation instead of re-querying the database (the
/// unit-of-work transaction has not committed yet at that point).
/// </summary>
internal static class InvoiceMapper
{
    public static InvoiceDetailDto ToDetail(Invoice i) =>
        new(
            i.Id, i.PatientId, i.ConsultationId, i.Status.ToString(), i.TotalAmount.Amount,
            i.PrimaryPaymentMethod?.ToString(), i.CreatedAtUtc,
            i.Lines.Select(l => new InvoiceLineDto(
                l.Id, l.ServiceCode, l.Description, l.Quantity,
                l.UnitPrice.Amount, l.LineTotal.Amount)).ToArray());
}
