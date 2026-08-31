using Jacana.PatientRegistration.Application.Abstractions;
using Jacana.PatientRegistration.Infrastructure.Persistence;
using Jacana.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace Jacana.PatientRegistration.Infrastructure.Services;

/// <summary>
/// Facility-scoped patient number generator: "PT-" + zero-padded sequential number,
/// scoped per facility within the patient schema. Derives the next number from the
/// highest existing sequence (not the row count) so soft-deleted records or gaps
/// never cause a duplicate-key collision on registration.
/// </summary>
public sealed class PatientNumberGenerator(PatientDbContext db) : IPatientNumberGenerator
{
    public async Task<string> NextAsync(FacilityId facilityId, CancellationToken ct = default)
    {
        var highest = await db.Patients
            .IgnoreQueryFilters() // soft-deleted rows still hold their number in the unique index
            .Where(p => p.FacilityId.Value == facilityId.Value)
            .Select(p => p.PatientNumber)
            .ToListAsync(ct);

        var maxSeq = highest
            .Select(n => n.StartsWith("PT-") && int.TryParse(n[3..], out var seq) ? seq : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"PT-{maxSeq + 1:D6}";
    }
}
