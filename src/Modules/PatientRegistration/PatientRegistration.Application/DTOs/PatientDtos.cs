namespace Jacana.PatientRegistration.Application.DTOs;

/// <summary>Lean read-model for list/search views.</summary>
public sealed record PatientSummaryDto(
    Guid Id,
    string PatientNumber,
    string FullName,
    DateOnly DateOfBirth,
    string? Phone,
    DateTime? LastVisitDate);

/// <summary>Full single-record projection including allergies/consents/next-of-kin.</summary>
public sealed record PatientDetailDto(
    Guid Id,
    string PatientNumber,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string MaritalStatus,
    string? Phone,
    string InsuranceType,
    string? InsuranceNumber,
    string ClinicType,
    string County,
    string? SubCounty,
    string? Ward,
    string? Line1,
    string Status,
    IReadOnlyList<AllergyDto> Allergies,
    IReadOnlyList<ConsentDto> Consents,
    IReadOnlyList<NextOfKinDto> NextOfKin,
    string? NationalId = null,
    Guid CreatedByUserId = default,
    string? CreatedByName = null,
    DateTime CreatedAtUtc = default,
    Guid? ModifiedByUserId = null,
    string? ModifiedByName = null,
    DateTime? ModifiedAtUtc = null);

public sealed record AllergyDto(Guid Id, string Substance, string Severity, string? Notes);
public sealed record ConsentDto(
    string Type,
    bool Granted,
    Guid RecordedByUserId,
    string? RecordedByName,
    DateTime RecordedAtUtc);
public sealed record NextOfKinDto(string FullName, string Relationship, string? Phone);

public sealed record RegisterPatientResponseDto(
    Guid Id,
    string PatientNumber,
    IReadOnlyList<DuplicateCandidateDto> DuplicateCandidates);

public sealed record DuplicateCandidateDto(
    Guid Id,
    string PatientNumber,
    string FullName,
    DateOnly DateOfBirth,
    string? Phone,
    string? NationalId);
