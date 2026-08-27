using Jacana.Inpatient.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Inpatient.Application.Features.Inpatient;

public sealed record AdmitPatientCommand(
    Guid PatientId,
    Guid AdmittingClinicianUserId,
    string WardName,
    string BedNumber)
    : ICommand<Result<AdmissionDetailDto>>;

public sealed record DischargePatientCommand(
    Guid AdmissionId)
    : ICommand<Result<AdmissionDetailDto>>;

public sealed record AddWardNoteCommand(
    Guid AdmissionId,
    string Content)
    : ICommand<Result<AdmissionDetailDto>>;
