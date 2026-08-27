using Jacana.PatientRegistration.Application.Abstractions;
using Jacana.PatientRegistration.Application.DTOs;
using Jacana.PatientRegistration.Domain;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.PatientRegistration.Application.Features.Patients;

public sealed class UpdatePatientDemographicsCommandHandler(IPatientRepository patients)
    : IRequestHandler<UpdatePatientDemographicsCommand, Result<PatientDetailDto>>
{
    public async Task<Result<PatientDetailDto>> Handle(UpdatePatientDemographicsCommand request, CancellationToken ct)
    {
        var patient = await patients.GetByIdAsync(request.PatientId, ct);
        if (patient is null) return Error.NotFound("Patient not found.");

        var phone = PhoneNumber.Create(request.Phone);
        if (phone.IsFailure) return phone.Error;
        var address = Address.Create(request.County, request.SubCounty, request.Ward, request.Line1);
        if (address.IsFailure) return address.Error;

        var result = patient.UpdateDemographics(
            request.FirstName, request.LastName, request.DateOfBirth,
            request.Gender, request.MaritalStatus, phone.Value, address.Value);
        if (result.IsFailure) return result.Error;

        await patients.UpdateAsync(patient, ct);

        var detail = await patients.GetDetailAsync(patient.Id, ct);
        return detail is null ? Error.NotFound("Patient not found after update.") : detail;
    }
}

public sealed class RegisterAllergyCommandHandler(IPatientRepository patients)
    : IRequestHandler<RegisterAllergyCommand, Result<PatientDetailDto>>
{
    public async Task<Result<PatientDetailDto>> Handle(RegisterAllergyCommand request, CancellationToken ct)
    {
        var patient = await patients.GetByIdAsync(request.PatientId, ct);
        if (patient is null) return Error.NotFound("Patient not found.");

        var result = patient.RegisterAllergy(request.Substance, request.Severity, request.Notes);
        if (result.IsFailure) return result.Error;

        await patients.UpdateAsync(patient, ct);
        return PatientMapper.ToDetail(patient);
    }
}

public sealed class RecordConsentCommandHandler(IPatientRepository patients)
    : IRequestHandler<RecordConsentCommand, Result<PatientDetailDto>>
{
    public async Task<Result<PatientDetailDto>> Handle(RecordConsentCommand request, CancellationToken ct)
    {
        var patient = await patients.GetByIdAsync(request.PatientId, ct);
        if (patient is null) return Error.NotFound("Patient not found.");

        var result = patient.RecordConsent(request.Type, request.Granted, Guid.Empty, DateTime.UtcNow);
        if (result.IsFailure) return result.Error;

        await patients.UpdateAsync(patient, ct);
        return PatientMapper.ToDetail(patient);
    }
}
