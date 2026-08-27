using Jacana.PatientRegistration.Application.Abstractions;
using Jacana.PatientRegistration.Infrastructure.Persistence;
using Jacana.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace Jacana.PatientRegistration.Infrastructure.Services;

/// <summary>
/// Facility-scoped patient number generator: "PT-" + zero-padded sequential number,
/// scoped per facility within the patient schema.
/// </summary>
public sealed class PatientNumberGenerator(PatientDbContext db) : IPatientNumberGenerator
{
    public async Task<string> NextAsync(FacilityId facilityId, CancellationToken ct = default)
    {
        var count = await db.Patients.CountAsync(p => p.FacilityId.Value == facilityId.Value, ct);
        return $"PT-{(count + 1):D6}";
    }
}
