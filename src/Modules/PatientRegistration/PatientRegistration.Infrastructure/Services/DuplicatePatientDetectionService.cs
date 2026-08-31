using Jacana.PatientRegistration.Domain;
using Jacana.PatientRegistration.Infrastructure.Persistence;
using Jacana.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace Jacana.PatientRegistration.Infrastructure.Services;

/// <summary>
/// Duplicate-patient detection: exact phone match, exact NationalId match, OR
/// fuzzy name + date-of-birth match. Returns candidates for staff confirmation;
/// never blocks or merges.
///
/// NationalId is encrypted at rest, so exact matching uses an in-memory equality
/// check on materialized rows plus a portable name/DOB fuzzy match.
/// </summary>
public sealed class DuplicatePatientDetectionService(PatientDbContext db)
    : IDuplicatePatientDetectionService
{
    public async Task<IReadOnlyList<Patient>> FindDuplicatesAsync(
        FacilityId facilityId, string firstName, string lastName,
        DateOnly dateOfBirth, PhoneNumber phone, NationalId? nationalId, CancellationToken ct = default)
    {
        var facilityPatients = db.Patients.AsNoTracking()
            .Where(p => p.FacilityId.Value == facilityId.Value);

        var candidates = new List<Patient>();

        // 1. Exact phone match (highest confidence) — same facility.
        var phoneValue = phone.Value;
        candidates.AddRange(await facilityPatients
            .Where(p => p.Phone.Value == phoneValue)
            .ToListAsync(ct));

        // 2. Exact NationalId match (highest confidence) — in-memory equality.
        if (nationalId is not null)
        {
            var withNational = await facilityPatients
                .Where(p => p.NationalId != null)
                .ToListAsync(ct);
            candidates.AddRange(withNational.Where(p => p.NationalId!.Value == nationalId.Value));
        }

        // 3. Fuzzy name (swapped or same order) + DOB match — provider-agnostic.
        var name = firstName.Trim().ToLowerInvariant();
        var surname = lastName.Trim().ToLowerInvariant();

        var nameDob = await facilityPatients
            .Where(p => p.DateOfBirth == dateOfBirth)
            .ToListAsync(ct);

        candidates.AddRange(nameDob.Where(p =>
        {
            var pf = p.FirstName.Trim().ToLowerInvariant();
            var pl = p.LastName.Trim().ToLowerInvariant();
            return (pf.Contains(name) && pl.Contains(surname))
                   || (pf.Contains(surname) && pl.Contains(name));
        }));

        return candidates
            .GroupBy(p => p.Id)
            .Select(g => g.First())
            .ToList();
    }
}
