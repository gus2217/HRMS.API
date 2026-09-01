using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Application.Abstractions;

/// <summary>
/// Persistence for the patient-scoped clinical summary (vitals, immunizations,
/// conditions). These are independent of the consultation aggregate.
/// </summary>
public interface IPatientClinicalRepository
{
    Task AddVitalSignAsync(VitalSign vitalSign, CancellationToken ct = default);
    Task AddImmunizationAsync(Immunization immunization, CancellationToken ct = default);
    Task AddConditionAsync(Condition condition, CancellationToken ct = default);

    Task<Condition?> GetConditionAsync(Guid id, CancellationToken ct = default);
    Task UpdateConditionAsync(Condition condition, CancellationToken ct = default);

    Task<IReadOnlyList<VitalSignDto>> GetVitalsAsync(Guid patientId, CancellationToken ct = default);
    Task<IReadOnlyList<ImmunizationDto>> GetImmunizationsAsync(Guid patientId, CancellationToken ct = default);
    Task<IReadOnlyList<ConditionDto>> GetConditionsAsync(Guid patientId, CancellationToken ct = default);
}
