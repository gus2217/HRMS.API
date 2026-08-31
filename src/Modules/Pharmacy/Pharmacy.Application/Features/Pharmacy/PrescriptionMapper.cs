using Jacana.Pharmacy.Application.DTOs;
using Jacana.Pharmacy.Domain;

namespace Jacana.Pharmacy.Application.Features.Pharmacy;

/// <summary>
/// Maps an in-memory <see cref="Prescription"/> aggregate to its detail DTO.
/// Handlers use this after mutation instead of re-querying the database (the
/// unit-of-work transaction has not committed yet at that point).
/// </summary>
internal static class PrescriptionMapper
{
    public static PrescriptionDetailDto ToDetail(Prescription p) =>
        new(
            p.Id, p.PatientId, p.ConsultationId, p.PrescribedByUserId,
            p.Status.ToString(), p.PrescribedAtUtc,
            p.Items.Select(i => new PrescriptionItemDto(
                i.Id, i.DrugId, string.Empty, string.Empty, string.Empty,
                i.DosageInstructions, i.Route, i.Frequency, i.DurationDays,
                i.QuantityPrescribed, i.QuantityDispensed, i.Status.ToString())).ToArray());
}
