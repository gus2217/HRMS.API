using Jacana.PatientRegistration.Domain;
using Jacana.SharedKernel.Domain;

namespace Jacana.PatientRegistration.Application.DTOs;

// HTTP request bindings for the patient endpoints (framework-agnostic records).

public sealed record RegisterPatientRequestDto(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    Gender Gender,
    MaritalStatus MaritalStatus,
    string Phone,
    string? NationalId,
    string? ShaNumber,
    string County,
    string? SubCounty,
    string? Ward,
    string? Line1,
    IReadOnlyList<NextOfKinRequestDto>? NextOfKin);

public sealed record NextOfKinRequestDto(string FullName, string Relationship, string Phone);

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
    string? Line1);

public sealed record RegisterAllergyRequestDto(
    string Substance,
    AllergySeverity Severity,
    string? Notes);

public sealed record RecordConsentRequestDto(
    ConsentType Type,
    bool Granted);
