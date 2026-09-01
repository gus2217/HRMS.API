using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Application.Abstractions;

/// <summary>Persistence for the patient-scoped clinical summary (vitals, immunizations, conditions).</summary>
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

    // ── Flags / attachments / diagnostic orders ───────────────────────────────
    Task AddPatientFlagAsync(PatientFlag flag, CancellationToken ct = default);
    Task<PatientFlag?> GetPatientFlagAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PatientFlagDto>> GetActiveFlagsAsync(Guid patientId, CancellationToken ct = default);
    Task<IReadOnlyList<PatientFlagDto>> GetAllFlagsAsync(Guid patientId, CancellationToken ct = default);

    Task AddAttachmentAsync(PatientAttachment attachment, CancellationToken ct = default);
    Task<PatientAttachment?> GetAttachmentAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PatientAttachmentDto>> GetAttachmentsAsync(Guid patientId, CancellationToken ct = default);
    Task DeleteAttachmentAsync(PatientAttachment attachment, CancellationToken ct = default);

    Task AddDiagnosticOrderAsync(DiagnosticOrder order, CancellationToken ct = default);
    Task<DiagnosticOrder?> GetDiagnosticOrderAsync(Guid id, CancellationToken ct = default);
    Task UpdateDiagnosticOrderAsync(DiagnosticOrder order, CancellationToken ct = default);
    Task<IReadOnlyList<DiagnosticOrderDto>> GetDiagnosticOrdersByPatientAsync(Guid patientId, CancellationToken ct = default);
    Task<IReadOnlyList<DiagnosticOrderDto>> GetDiagnosticOrdersByConsultationAsync(Guid consultationId, CancellationToken ct = default);
}
