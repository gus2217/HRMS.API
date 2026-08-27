using Jacana.Inpatient.Domain;
using Jacana.SharedKernel.Domain;
using Xunit;

namespace Jacana.Tests.Unit.Inpatient;

public class AdmissionTests
{
    private static Admission NewAdmission()
        => Admission.Admit(Guid.NewGuid(), FacilityId.New(), Guid.NewGuid(),
            Guid.NewGuid(), "Ward A", "Bed 1", DateTime.UtcNow).Value;

    [Fact]
    public void Admit_publishes_patient_admitted_event()
    {
        var admission = NewAdmission();
        Assert.Single(admission.DomainEvents);
        Assert.IsType<PatientAdmittedDomainEvent>(admission.DomainEvents.Single());
    }

    [Fact]
    public void Discharge_publishes_event_and_sets_timestamp()
    {
        var admission = NewAdmission();
        var result = admission.Discharge(DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(AdmissionStatus.Discharged, admission.Status);
        Assert.NotNull(admission.DischargedAtUtc);
        Assert.Equal(2, admission.DomainEvents.Count);
    }

    [Fact]
    public void Cannot_discharge_twice()
    {
        var admission = NewAdmission();
        admission.Discharge(DateTime.UtcNow);
        Assert.True(admission.Discharge(DateTime.UtcNow).IsFailure);
    }

    [Fact]
    public void Cannot_add_note_to_discharged()
    {
        var admission = NewAdmission();
        admission.Discharge(DateTime.UtcNow);
        Assert.True(admission.AddWardNote("note", Guid.NewGuid(), DateTime.UtcNow).IsFailure);
    }
}
