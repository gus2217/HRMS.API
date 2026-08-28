using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Application.Abstractions;

public interface IConsultationRepository
{
    Task<Consultation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Consultation consultation, CancellationToken ct = default);
    Task UpdateAsync(Consultation consultation, CancellationToken ct = default);
    Task<ConsultationDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ConsultationSummaryDto>> GetByPatientAsync(Guid patientId, CancellationToken ct = default);
    Task<PatientClinicalHistoryDto?> GetPatientHistoryAsync(Guid patientId, CancellationToken ct = default);
    Task<IReadOnlyList<ConsultationSummaryDto>> SearchAsync(
        string? status, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(string? status, CancellationToken ct = default);
}
