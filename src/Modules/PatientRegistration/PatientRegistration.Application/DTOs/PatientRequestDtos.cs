using Jacana.PatientRegistration.Domain;
using Jacana.SharedKernel.Domain;

namespace Jacana.PatientRegistration.Application.DTOs;

// HTTP request bindings for the patient endpoints (framework-agnostic records).

public sealed record RegistryLookupRequestDto(string NationalId);

public sealed record RegisterPatientRequestDto(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    Gender Gender,
    string Phone,
    string? NationalId,
    InsuranceType InsuranceType,
    string? InsuranceNumber,
    ClinicType ClinicType,
    string County,
    string? SubCounty,
    string? Ward,
    string? Line1,
    string? MiddleName = null,
    string? Village = null,
    string? Landmark = null,
    EducationLevel? EducationLevel = null,
    string? Occupation = null,
    string? AlternativePhone = null,
    string? NationalRegistryNumber = null);

public sealed record UpdatePatientDemographicsRequestDto(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    Gender Gender,
    MaritalStatus MaritalStatus,
    string Phone,
    string County,
    string? SubCounty,
    string? Ward,
    string? Line1,
    string? MiddleName = null,
    string? Village = null,
    string? Landmark = null,
    EducationLevel? EducationLevel = null,
    string? Occupation = null,
    string? AlternativePhone = null);

public sealed record RegisterAllergyRequestDto(
    string Substance,
    AllergySeverity Severity,
    string? Notes);

public sealed record RecordConsentRequestDto(
    ConsentType Type,
    bool Granted);
