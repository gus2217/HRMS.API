using Jacana.PatientRegistration.Application.Abstractions;
using Jacana.PatientRegistration.Application.DTOs;
using Jacana.PatientRegistration.Domain;
using Jacana.PatientRegistration.Infrastructure.Persistence;
using Jacana.SharedKernel.Domain;
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
        string? search, int pageNumber, int pageSize, string? sort = null, CancellationToken ct = default)
    {
        var term = search?.Trim();
        var query = db.Patients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(term))
            query = await BuildSearchQueryAsync(query, term, ct);

        query = string.Equals(sort, "latest", StringComparison.OrdinalIgnoreCase)
            ? query.OrderByDescending(p => p.CreatedAtUtc)
            : query.OrderBy(p => p.LastName).ThenBy(p => p.FirstName);

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatientSummaryDto(
                p.Id, p.PatientNumber, p.FirstName + " " + p.LastName, p.DateOfBirth,
                p.Phone.Value, null))
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(string? search, CancellationToken ct = default)
    {
        var term = search?.Trim();
        var query = db.Patients.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(term))
            query = await BuildSearchQueryAsync(query, term, ct);
        return await query.CountAsync(ct);
    }

    /// <summary>
    /// Matches a free-text term against name, patient number, phone (any Kenyan
    /// format) and national ID simultaneously. National ID is encrypted at rest,
    /// so exact matching resolves IDs in memory — guarded to digit-only terms that
    /// are not already a valid phone.
    /// </summary>
    private async Task<IQueryable<Patient>> BuildSearchQueryAsync(
        IQueryable<Patient> query, string term, CancellationToken ct)
    {
        var lower = term.ToLowerInvariant();
        var phone = PhoneNumber.TryNormalize(term);

        var nationalIds = await ResolveNationalIdMatchesAsync(term, phone, ct);

        return query.Where(p =>
            p.FirstName.ToLower().Contains(lower)
            || p.LastName.ToLower().Contains(lower)
            || (p.FirstName + " " + p.LastName).ToLower().Contains(lower)
            || p.PatientNumber.ToLower().Contains(lower)
            || (phone != null && p.Phone.Value == phone)
            || nationalIds.Contains(p.Id));
    }

    private async Task<IReadOnlyList<Guid>> ResolveNationalIdMatchesAsync(
        string term, string? phone, CancellationToken ct)
    {
        if (phone is not null) return [];
        if (!term.All(char.IsDigit) || term.Length < 6 || term.Length > 12) return [];

        var rows = await db.Patients.AsNoTracking()
            .Where(p => p.NationalId != null)
            .Select(p => new { p.Id, p.NationalId })
            .ToListAsync(ct);

        return rows
            .Where(r => r.NationalId!.Value == term)
            .Select(r => r.Id)
            .ToList();
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
            p.Gender.ToString(), p.MaritalStatus.ToString(), p.Phone.Value,
            p.InsuranceType.ToString(), p.InsuranceNumber, p.ClinicType.ToString(),
            p.Address.County, p.Address.SubCounty, p.Address.Ward, p.Address.Line1,
            p.Status.ToString(),
            p.Allergies.Select(a => new AllergyDto(a.Substance, a.Severity.ToString(), a.Notes)).ToArray(),
            p.Consents.Select(c => new ConsentDto(c.Type.ToString(), c.Granted, c.RecordedAtUtc)).ToArray(),
            p.NextOfKin.Select(k => new NextOfKinDto(k.FullName, k.Relationship, k.Phone.Value)).ToArray());
    }

    public async Task<IReadOnlyList<Patient>> FindByPhoneOrNationalIdAsync(
        FacilityId facilityId, string? phone, string? nationalId, CancellationToken ct = default)
    {
        var query = db.Patients.AsNoTracking()
            .Where(p => p.FacilityId.Value == facilityId.Value);

        var candidates = new List<Patient>();

        // Exact phone match (any Kenyan format) — high confidence.
        if (!string.IsNullOrWhiteSpace(phone))
        {
            var phoneValue = PhoneNumber.TryNormalize(phone);
            if (phoneValue is not null)
                candidates.AddRange(await query
                    .Where(p => p.Phone.Value == phoneValue)
                    .ToListAsync(ct));
        }

        // Exact NationalId match — in-memory equality (encrypted at rest).
        if (!string.IsNullOrWhiteSpace(nationalId))
        {
            var withNational = await query
                .Where(p => p.NationalId != null)
                .ToListAsync(ct);
            var parsed = NationalId.Create(nationalId.Trim());
            if (parsed.IsSuccess)
                candidates.AddRange(withNational.Where(p => p.NationalId!.Value == parsed.Value.Value));
        }

        return candidates
            .GroupBy(p => p.Id)
            .Select(g => g.First())
            .ToList();
    }
}
