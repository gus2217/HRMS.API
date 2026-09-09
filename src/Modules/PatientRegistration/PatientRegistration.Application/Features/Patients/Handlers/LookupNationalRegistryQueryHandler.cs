using Jacana.PatientRegistration.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.PatientRegistration.Application.Features.Patients;

/// <summary>
/// National client-registry (NUPI) lookup by Kenyan National ID. Used at the
/// front desk before registration: a found client means the record can be
/// pulled/prefilled locally; an absent client means a fresh local creation.
/// The registry client never throws — connectivity/auth problems return a
/// RegistryLookupResult with Found=false and a human-readable message so the
/// desk can fall back to manual registration.
/// </summary>
public sealed class LookupNationalRegistryQueryHandler(IClientRegistryLookup registry)
    : IRequestHandler<LookupNationalRegistryQuery, Result<RegistryLookupResult>>
{
    public async Task<Result<RegistryLookupResult>> Handle(LookupNationalRegistryQuery request, CancellationToken ct)
    {
        var nationalId = NationalId.Create(request.NationalId);
        if (nationalId.IsFailure) return nationalId.Error;

        var result = await registry.LookupByNationalIdAsync(nationalId.Value.Value, ct);
        return Result.Success(result);
    }
}
