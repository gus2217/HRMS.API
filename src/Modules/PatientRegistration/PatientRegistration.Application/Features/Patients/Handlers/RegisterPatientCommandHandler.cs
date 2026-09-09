using Jacana.PatientRegistration.Application.Abstractions;
using Jacana.PatientRegistration.Application.DTOs;
using Jacana.PatientRegistration.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.PatientRegistration.Application.Features.Patients;

/// <summary>
/// Registers a patient. Runs duplicate detection first and, if candidates exist,
/// returns them for staff confirmation rather than blocking or silently merging.
/// </summary>
public sealed class RegisterPatientCommandHandler(
    IPatientRepository patients,
    IDuplicatePatientDetectionService duplicates,
    IPatientNumberGenerator numberGenerator,
    ICurrentUser currentUser)
    : IRequestHandler<RegisterPatientCommand, Result<RegisterPatientResponseDto>>
{
    public async Task<Result<RegisterPatientResponseDto>> Handle(RegisterPatientCommand request, CancellationToken ct)
    {
        var phoneResult = PhoneNumber.Create(request.Phone);
        if (phoneResult.IsFailure) return phoneResult.Error;

        var addressResult = Address.Create(request.County, request.SubCounty, request.Ward, request.Line1,
            request.Village, request.Landmark);
        if (addressResult.IsFailure) return addressResult.Error;

        string? alternativePhone = null;
        if (!string.IsNullOrWhiteSpace(request.AlternativePhone))
        {
            var alt = PhoneNumber.TryNormalize(request.AlternativePhone);
            if (alt is null)
                return Error.Validation("Alternative phone is not a valid Kenyan number.");
            alternativePhone = alt;
        }

        NationalId? nationalId = null;
        if (!string.IsNullOrWhiteSpace(request.NationalId))
        {
            var nid = NationalId.Create(request.NationalId);
            if (nid.IsFailure) return nid.Error;
            nationalId = nid.Value;
        }

        // Duplicate detection → return candidates for confirmation, never silent merge.
        var candidates = await duplicates.FindDuplicatesAsync(
            currentUser.FacilityId, request.FirstName, request.LastName,
            request.DateOfBirth, phoneResult.Value, nationalId, ct);

        if (candidates.Count > 0)
        {
            return new RegisterPatientResponseDto(Guid.Empty, string.Empty,
                candidates.Select(c => new DuplicateCandidateDto(
                    c.Id, c.PatientNumber, FullName(c), c.DateOfBirth,
                    c.Phone.Value, c.NationalId?.Value)).ToArray());
        }

        var patientNumber = await numberGenerator.NextAsync(currentUser.FacilityId, ct);

        var patientResult = Patient.Register(
            Guid.NewGuid(),
            currentUser.FacilityId,
            patientNumber,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            phoneResult.Value,
            addressResult.Value,
            request.InsuranceType,
            request.InsuranceNumber,
            request.ClinicType,
            request.MiddleName,
            alternativePhone,
            request.EducationLevel,
            request.Occupation);

        if (patientResult.IsFailure) return patientResult.Error;

        var patient = patientResult.Value;

        if (nationalId is not null)
            patient.SetNationalId(nationalId);

        if (!string.IsNullOrWhiteSpace(request.NationalRegistryNumber))
            patient.SetNationalRegistryNumber(request.NationalRegistryNumber!.Trim());

        await patients.AddAsync(patient, ct);

        return new RegisterPatientResponseDto(patient.Id, patient.PatientNumber, []);
    }

    private static string FullName(Patient p)
        => string.IsNullOrWhiteSpace(p.MiddleName)
            ? $"{p.FirstName} {p.LastName}"
            : $"{p.FirstName} {p.MiddleName} {p.LastName}";
}
