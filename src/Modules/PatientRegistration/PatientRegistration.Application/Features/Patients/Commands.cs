using Jacana.PatientRegistration.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.PatientRegistration.Application.Features.Patients;

public sealed record RegisterPatientCommand(
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
    IReadOnlyList<NextOfKinInput>? NextOfKin)
    : ICommand<Result<RegisterPatientResponseDto>>;

public sealed record NextOfKinInput(string FullName, string Relationship, string Phone);

public sealed record UpdatePatientDemographicsCommand(
    Guid PatientId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    Gender Gender,
    MaritalStatus MaritalStatus,
    string Phone,
    string County,
    string? SubCounty,
    string? Ward,
    string? Line1)
    : ICommand<Result<PatientDetailDto>>;

public sealed record RegisterAllergyCommand(
    Guid PatientId,
    string Substance,
    Domain.AllergySeverity Severity,
    string? Notes)
    : ICommand<Result<PatientDetailDto>>;

public sealed record RecordConsentCommand(
    Guid PatientId,
    Domain.ConsentType Type,
    bool Granted)
    : ICommand<Result<PatientDetailDto>>;
