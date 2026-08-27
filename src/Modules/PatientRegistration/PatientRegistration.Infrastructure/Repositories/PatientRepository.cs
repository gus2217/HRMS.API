using Jacana.PatientRegistration.Application.Abstractions;
using Jacana.PatientRegistration.Application.DTOs;
using Jacana.PatientRegistration.Domain;
using Jacana.PatientRegistration.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Jacana.SharedKernel.Infrastructure.Persistence;

namespace Jacana.PatientRegistration.Infrastructure.Repositories;

public sealed class PatientRepository(PatientDbContext db) : IPatientRepository
{
    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Patients
            .Include(p => p.Allergies)
            .Include(p => p.Consents)
            .Include(p => p.NextOfKin)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Patient?> GetByPatientNumberAsync(string patientNumber, CancellationToken ct = default)
        => await db.Patients.FirstOrDefaultAsync(p => p.PatientNumber == patientNumber, ct);

    public Task AddAsync(Patient patient, CancellationToken ct = default)
    {
        db.Patients.Add(patient);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Patient patient, CancellationToken ct = default)
    {
        // Aggregate already tracked from GetByIdAsync. New children carry
        // client-generated keys; EF DetectChanges would classify them as Modified
        // (phantom UPDATE, 0 rows). Mark them Added explicitly while still Detached.
        db.MarkNewChildrenAdded(patient);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<PatientSummaryDto>> SearchAsync(
        string? search, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Patients.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.FirstName.Contains(search) || p.LastName.Contains(search) || p.PatientNumber.Contains(search));

        return await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatientSummaryDto(
                p.Id, p.PatientNumber, p.FirstName + " " + p.LastName, p.DateOfBirth,
                p.Phone.Value, null))
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? search, CancellationToken ct = default)
    {
        var query = db.Patients.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.FirstName.Contains(search) || p.LastName.Contains(search) || p.PatientNumber.Contains(search));
        return query.CountAsync(ct);
    }

    public async Task<PatientDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var p = await db.Patients.AsNoTracking()
            .Include(x => x.Allergies)
            .Include(x => x.Consents)
            .Include(x => x.NextOfKin)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (p is null) return null;

        return new PatientDetailDto(
            p.Id, p.PatientNumber, p.FirstName, p.LastName, p.DateOfBirth,
            p.Gender.ToString(), p.MaritalStatus.ToString(), p.Phone.Value, p.ShaNumber,
            p.Address.County, p.Address.SubCounty, p.Address.Ward, p.Address.Line1,
            p.Status.ToString(),
            p.Allergies.Select(a => new AllergyDto(a.Substance, a.Severity.ToString(), a.Notes)).ToArray(),
            p.Consents.Select(c => new ConsentDto(c.Type.ToString(), c.Granted, c.RecordedAtUtc)).ToArray(),
            p.NextOfKin.Select(k => new NextOfKinDto(k.FullName, k.Relationship, k.Phone.Value)).ToArray());
    }
}
