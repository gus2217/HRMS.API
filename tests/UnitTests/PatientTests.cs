using Jacana.PatientRegistration.Domain;
using Jacana.SharedKernel.Domain;
using Xunit;

namespace Jacana.Tests.Unit.PatientRegistration;

public class AddressTests
{
    [Fact]
    public void County_is_required()
    {
        Assert.True(Address.Create("").IsFailure);
        Assert.True(Address.Create("  ").IsFailure);
    }

    [Fact]
    public void Valid_address_succeeds()
    {
        var result = Address.Create("Nairobi", "Westlands", "Parklands", "123 Main St");
        Assert.True(result.IsSuccess);
        Assert.Equal("Nairobi", result.Value.County);
    }
}

public class PatientTests
{
    private static Patient CreatePatient()
    {
        var phone = PhoneNumber.Create("+254712345678").Value;
        var address = Address.Create("Nairobi").Value;
        return Patient.Register(
            Guid.NewGuid(), FacilityId.New(), "PT-000001", "Jane", "Doe",
            new DateOnly(1990, 5, 12), Gender.Female, MaritalStatus.Single,
            phone, address).Value;
    }

    [Fact]
    public void Register_requires_names()
    {
        var phone = PhoneNumber.Create("+254712345678").Value;
        var address = Address.Create("Nairobi").Value;

        Assert.True(Patient.Register(Guid.NewGuid(), FacilityId.New(), "PT-1", "", "Doe",
            new DateOnly(1990, 1, 1), Gender.Female, MaritalStatus.Single, phone, address).IsFailure);
        Assert.True(Patient.Register(Guid.NewGuid(), FacilityId.New(), "PT-1", "Jane", "",
            new DateOnly(1990, 1, 1), Gender.Female, MaritalStatus.Single, phone, address).IsFailure);
    }

    [Fact]
    public void RegisterAllergy_adds_to_collection()
    {
        var patient = CreatePatient();
        var result = patient.RegisterAllergy("Penicillin", AllergySeverity.Severe, "Anaphylaxis");
        Assert.True(result.IsSuccess);
        Assert.Single(patient.Allergies);
    }

    [Fact]
    public void RegisterAllergy_requires_substance()
    {
        var patient = CreatePatient();
        Assert.True(patient.RegisterAllergy("", AllergySeverity.Mild, null).IsFailure);
    }

    [Fact]
    public void UpdateDemographics_cannot_null_identity_fields()
    {
        var patient = CreatePatient();
        var phone = PhoneNumber.Create("+254712345678").Value;
        var address = Address.Create("Nairobi").Value;

        Assert.True(patient.UpdateDemographics("", "Doe", new DateOnly(1990, 1, 1),
            Gender.Female, MaritalStatus.Single, phone, address).IsFailure);
        Assert.True(patient.UpdateDemographics("Jane", "", new DateOnly(1990, 1, 1),
            Gender.Female, MaritalStatus.Single, phone, address).IsFailure);
    }

    [Fact]
    public void RecordConsent_adds_record()
    {
        var patient = CreatePatient();
        var result = patient.RecordConsent(ConsentType.TreatmentConsent, true, Guid.NewGuid(), DateTime.UtcNow);
        Assert.True(result.IsSuccess);
        Assert.Single(patient.Consents);
    }

    [Fact]
    public void Patient_is_soft_deletable_entity()
    {
        var patient = CreatePatient();
        Assert.IsAssignableFrom<ISoftDelete>(patient);
        Assert.IsAssignableFrom<IAuditable>(patient);
    }
}
