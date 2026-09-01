using Jacana.Inpatient.Domain;
using Jacana.Laboratory.Domain;
using Jacana.SharedKernel.Domain;
using Xunit;

namespace Jacana.Tests.Unit.Inpatient;

public class AdmissionTests
{
    private static Admission NewAdmission()
        => Admission.Admit(Guid.NewGuid(), FacilityId.New(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), "Ward A", "Bed 1", "Pneumonia", Guid.NewGuid(), DateTime.UtcNow).Value;

    private static WardMedicalRecord CompleteRecord(Admission admission)
        => WardMedicalRecord.Create(admission.Id, Guid.NewGuid(), DateTime.UtcNow,
            null, null, null, null, null, null, null,
            "Fever", "Crackles", "Community-acquired pneumonia", "Amoxicillin 1g TDS x5d").Value;

    private static WardMedicalRecord IncompleteRecord(Admission admission)
        => WardMedicalRecord.Create(admission.Id, Guid.NewGuid(), DateTime.UtcNow,
            null, null, null, null, null, null, null,
            "Fever", "Crackles", null, null).Value;

    [Fact]
    public void Admit_publishes_patient_admitted_event()
    {
        var admission = NewAdmission();
        Assert.Single(admission.DomainEvents);
        Assert.IsType<PatientAdmittedDomainEvent>(admission.DomainEvents.Single());
    }

    [Fact]
    public void Discharge_requires_complete_medical_record_and_cleared_bill()
    {
        var admission = NewAdmission();

        // No records yet → discharge blocked.
        var blocked = admission.Discharge(billCleared: true, DateTime.UtcNow);
        Assert.True(blocked.IsFailure);

        // Incomplete record still blocks.
        admission.AddMedicalRecord(IncompleteRecord(admission));
        Assert.True(admission.Discharge(billCleared: true, DateTime.UtcNow).IsFailure);

        // Complete record but unpaid bill still blocks.
        admission.AddMedicalRecord(CompleteRecord(admission));
        Assert.True(admission.Discharge(billCleared: false, DateTime.UtcNow).IsFailure);

        // Complete record + cleared bill → success.
        var result = admission.Discharge(billCleared: true, DateTime.UtcNow);
        Assert.True(result.IsSuccess);
        Assert.Equal(AdmissionStatus.Discharged, admission.Status);
        Assert.NotNull(admission.DischargedAtUtc);
        Assert.Equal(2, admission.DomainEvents.Count);
    }

    [Fact]
    public void Cannot_discharge_twice()
    {
        var admission = NewAdmission();
        admission.AddMedicalRecord(CompleteRecord(admission));
        admission.Discharge(billCleared: true, DateTime.UtcNow);
        Assert.True(admission.Discharge(billCleared: true, DateTime.UtcNow).IsFailure);
    }

    [Fact]
    public void Cannot_add_note_to_discharged()
    {
        var admission = NewAdmission();
        admission.AddMedicalRecord(CompleteRecord(admission));
        admission.Discharge(billCleared: true, DateTime.UtcNow);
        Assert.True(admission.AddWardNote("note", Guid.NewGuid(), DateTime.UtcNow).IsFailure);
    }

    [Fact]
    public void Cannot_add_medical_record_to_discharged()
    {
        var admission = NewAdmission();
        admission.AddMedicalRecord(CompleteRecord(admission));
        admission.Discharge(billCleared: true, DateTime.UtcNow);

        Assert.True(admission.AddMedicalRecord(CompleteRecord(admission)).IsFailure);
    }
}

public class WardTests
{
    [Fact]
    public void Ward_requires_name_and_positive_beds()
    {
        Assert.True(Ward.Create(FacilityId.New(), " ", WardType.General, 10).IsFailure);
        Assert.True(Ward.Create(FacilityId.New(), "General", WardType.General, 0).IsFailure);
    }

    [Fact]
    public void Ward_can_deactivate()
    {
        var ward = Ward.Create(FacilityId.New(), "ICU", WardType.Icu, 6).Value;
        ward.Deactivate();
        Assert.False(ward.IsActive);
    }
}

public class AdmissionTransferTests
{
    [Fact]
    public void Transfer_updates_ward_and_publishes_event()
    {
        var admission = NewAdmission();
        var result = admission.Transfer(Guid.NewGuid(), "ICU", "Bed 2", DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal("ICU", admission.WardName);
        Assert.Equal("Bed 2", admission.BedNumber);
        Assert.Contains(admission.DomainEvents, e => e is PatientTransferredDomainEvent);
    }

    [Fact]
    public void Cannot_transfer_discharged_admission()
    {
        var admission = NewAdmission();
        admission.AddMedicalRecord(CompleteRecord(admission));
        admission.Discharge(billCleared: true, DateTime.UtcNow);

        Assert.True(admission.Transfer(Guid.NewGuid(), "ICU", "Bed 2", DateTime.UtcNow).IsFailure);
    }

    private static Admission NewAdmission()
        => Admission.Admit(Guid.NewGuid(), FacilityId.New(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), "Ward A", "Bed 1", "Pneumonia", Guid.NewGuid(), DateTime.UtcNow).Value;

    private static WardMedicalRecord CompleteRecord(Admission admission)
        => WardMedicalRecord.Create(admission.Id, Guid.NewGuid(), DateTime.UtcNow,
            null, null, null, null, null, null, null,
            "Fever", "Crackles", "Community-acquired pneumonia", "Amoxicillin 1g TDS x5d").Value;
}

public class LabOrderCancelTests
{
    [Fact]
    public void Lab_order_can_be_cancelled_when_pending()
    {
        var order = LabOrder.Create(Guid.NewGuid(), FacilityId.New(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow).Value;
        order.AddTest("FBC", "Full Blood Count");

        var result = order.Cancel(Guid.NewGuid(), DateTime.UtcNow, "Ordered in error");
        Assert.True(result.IsSuccess);
        Assert.Equal(LabOrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Completed_lab_order_cannot_be_cancelled()
    {
        var order = LabOrder.Create(Guid.NewGuid(), FacilityId.New(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow).Value;
        var testId = Guid.NewGuid();
        // mark complete by recording the only test
        order.AddTest("FBC", "Full Blood Count");
        var t = order.Tests.First();
        order.RecordTestResult(t.Id, "12.5", "g/dL", null, false, Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(order.Cancel(Guid.NewGuid(), DateTime.UtcNow).IsFailure);
    }
}
