using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Domain;
using Xunit;

namespace Jacana.Tests.Unit.Clinical;

public class PatientClinicalSummaryTests
{
    private static readonly FacilityId Facility = FacilityId.New();
    private static readonly Guid Patient = Guid.NewGuid();
    private static readonly Guid Recorder = Guid.NewGuid();

    [Fact]
    public void VitalSign_computes_bmi_from_weight_and_height()
    {
        var v = VitalSign.Record(Facility, Patient, null, null, null, null, null, null,
            70m, 175m, Recorder, DateTime.UtcNow).Value;

        Assert.Equal(22.9m, v.Bmi);
    }

    [Fact]
    public void VitalSign_rejects_implausible_temperature()
    {
        var result = VitalSign.Record(Facility, Patient, 50m, null, null, null, null, null,
            null, null, Recorder, DateTime.UtcNow);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void VitalSign_requires_patient_and_recorder()
    {
        Assert.True(VitalSign.Record(Facility, Guid.Empty, null, null, null, null, null, null,
            null, null, Recorder, DateTime.UtcNow).IsFailure);
        Assert.True(VitalSign.Record(Facility, Patient, null, null, null, null, null, null,
            null, null, Guid.Empty, DateTime.UtcNow).IsFailure);
    }

    [Fact]
    public void Immunization_requires_vaccine_name()
    {
        var result = Immunization.Record(Facility, Patient, "  ", 1, DateTime.UtcNow, null,
            null, null, null, Recorder, DateTime.UtcNow);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Immunization_rejects_next_due_before_administered()
    {
        var administered = DateTime.UtcNow.Date;
        var result = Immunization.Record(Facility, Patient, "BCG", 1, administered,
            administered.AddDays(-1), null, null, null, Recorder, DateTime.UtcNow);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Immunization_requires_dose_number_at_least_one()
    {
        var result = Immunization.Record(Facility, Patient, "BCG", 0, DateTime.UtcNow, null,
            null, null, null, Recorder, DateTime.UtcNow);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Condition_starts_active_and_resolves()
    {
        var c = Condition.Add(Facility, Patient, "E11.9", "Type 2 diabetes", DateTime.UtcNow,
            Recorder, DateTime.UtcNow).Value;

        Assert.Equal(ConditionStatus.Active, c.Status);

        var resolve = c.Resolve(DateTime.UtcNow);
        Assert.True(resolve.IsSuccess);
        Assert.Equal(ConditionStatus.Resolved, c.Status);
        Assert.NotNull(c.ResolvedDate);
    }

    [Fact]
    public void Condition_cannot_resolve_twice()
    {
        var c = Condition.Add(Facility, Patient, null, "Asthma", DateTime.UtcNow,
            Recorder, DateTime.UtcNow).Value;
        c.Resolve(DateTime.UtcNow);

        Assert.True(c.Resolve(DateTime.UtcNow).IsFailure);
    }

    [Fact]
    public void Condition_requires_description()
    {
        var result = Condition.Add(Facility, Patient, null, " ", DateTime.UtcNow,
            Recorder, DateTime.UtcNow);
        Assert.True(result.IsFailure);
    }
}
