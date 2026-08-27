using Jacana.SharedKernel.Domain;
using NextOfKinEntity = Jacana.PatientRegistration.Domain.NextOfKin;

namespace Jacana.PatientRegistration.Domain;

/// <summary>
/// The core patient record. Enforces identity-field invariants and exposes behavior
/// methods (no public setters on business-rule properties).
/// </summary>
public sealed class Patient : AggregateRoot<Guid>
{
    private readonly List<NextOfKin> _nextOfKin = new();
    private readonly List<Allergy> _allergies = new();
    private readonly List<ConsentRecord> _consents = new();

    private Patient() { } // EF

    private Patient(
        Guid id,
        FacilityId facilityId,
        string patientNumber,
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        Gender gender,
        MaritalStatus maritalStatus,
        PhoneNumber phone,
        Address address)
        : base(id)
    {
        FacilityId = facilityId;
        PatientNumber = patientNumber;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        MaritalStatus = maritalStatus;
        Phone = phone;
        Address = address;
        Status = RecordStatus.Active;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public string PatientNumber { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public MaritalStatus MaritalStatus { get; private set; }
    public PhoneNumber Phone { get; private set; } = null!;
    public NationalId? NationalId { get; private set; }
    public string? ShaNumber { get; private set; }
    public Address Address { get; private set; } = null!;
    public RecordStatus Status { get; private set; }

    public IReadOnlyCollection<NextOfKin> NextOfKin => _nextOfKin.AsReadOnly();
    public IReadOnlyCollection<Allergy> Allergies => _allergies.AsReadOnly();
    public IReadOnlyCollection<ConsentRecord> Consents => _consents.AsReadOnly();

    public static Result<Patient> Register(
        Guid id,
        FacilityId facilityId,
        string patientNumber,
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        Gender gender,
        MaritalStatus maritalStatus,
        PhoneNumber phone,
        Address address)
    {
        if (string.IsNullOrWhiteSpace(patientNumber))
            return Error.Validation("Patient number is required.");
        if (string.IsNullOrWhiteSpace(firstName))
            return Error.Validation("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            return Error.Validation("Last name is required.");

        return new Patient(id, facilityId, patientNumber, firstName.Trim(), lastName.Trim(),
            dateOfBirth, gender, maritalStatus, phone, address);
    }

    public Result RegisterAllergy(string substance, AllergySeverity severity, string? notes)
    {
        var allergy = Allergy.Create(substance, severity, notes);
        if (allergy.IsFailure) return allergy.Error;
        _allergies.Add(allergy.Value);
        return Result.Success();
    }

    public Result RecordConsent(ConsentType type, bool granted, Guid recordedByUserId, DateTime recordedAtUtc)
    {
        _consents.Add(ConsentRecord.Create(type, granted, recordedByUserId, recordedAtUtc));
        return Result.Success();
    }

    public Result AddNextOfKin(string fullName, string relationship, PhoneNumber phone)
    {
        var kin = NextOfKinEntity.Create(fullName, relationship, phone);
        if (kin.IsFailure) return kin.Error;
        _nextOfKin.Add(kin.Value);
        return Result.Success();
    }

    public Result SetNationalId(NationalId nationalId)
    {
        NationalId = nationalId;
        return Result.Success();
    }

    public Result SetShaNumber(string? shaNumber)
    {
        if (shaNumber is not null && shaNumber.Length > 50)
            return Error.Validation("SHA number is too long.");
        ShaNumber = shaNumber;
        return Result.Success();
    }

    /// <summary>
    /// Updates demographics. Guards: required identity fields cannot be nulled out.
    /// </summary>
    public Result UpdateDemographics(string firstName, string lastName, DateOnly dateOfBirth,
        Gender gender, MaritalStatus maritalStatus, PhoneNumber phone, Address address)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Error.Validation("First name cannot be empty.");
        if (string.IsNullOrWhiteSpace(lastName))
            return Error.Validation("Last name cannot be empty.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        DateOfBirth = dateOfBirth;
        Gender = gender;
        MaritalStatus = maritalStatus;
        Phone = phone;
        Address = address;
        return Result.Success();
    }
}
