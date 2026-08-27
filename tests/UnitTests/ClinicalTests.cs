using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Domain;
using Xunit;

namespace Jacana.Tests.Unit.Clinical;

public class ConsultationStateMachineTests
{
    private static Consultation StartConsultation()
        => Consultation.Start(Guid.NewGuid(), FacilityId.New(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow).Value;

    [Fact]
    public void Start_requires_patient_and_clinician()
    {
        Assert.True(Consultation.Start(Guid.NewGuid(), FacilityId.New(), Guid.Empty, Guid.NewGuid(), DateTime.UtcNow).IsFailure);
        Assert.True(Consultation.Start(Guid.NewGuid(), FacilityId.New(), Guid.NewGuid(), Guid.Empty, DateTime.UtcNow).IsFailure);
    }

    [Fact]
    public void Full_workflow_reaches_completed()
    {
        var c = StartConsultation();

        c.RecordTriage(TriageData.Create(37.0m, "120/80", 72, 16, 70m).Value);
        c.AdvanceTo(ConsultationStatus.AwaitingClinician);
        c.AdvanceTo(ConsultationStatus.InConsultation);
        c.RecordDiagnosis("J45", "Asthma", true);
        var complete = c.Complete(DateTime.UtcNow);

        Assert.True(complete.IsSuccess);
        Assert.Equal(ConsultationStatus.Completed, c.Status);
        Assert.NotNull(c.CompletedAtUtc);
    }

    [Fact]
    public void Cannot_complete_without_diagnosis()
    {
        var c = StartConsultation();
        c.RecordTriage(TriageData.Create(null, null, null, null, null).Value);
        c.AdvanceTo(ConsultationStatus.AwaitingClinician);
        c.AdvanceTo(ConsultationStatus.InConsultation);
        c.AdvanceTo(ConsultationStatus.DiagnosisRecorded);

        var result = c.Complete(DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(ConsultationStatus.DiagnosisRecorded, c.Status);
    }

    [Fact]
    public void Cannot_complete_before_diagnosis_recorded_status()
    {
        var c = StartConsultation();
        c.RecordTriage(TriageData.Create(null, null, null, null, null).Value);
        c.AdvanceTo(ConsultationStatus.AwaitingClinician);
        c.AdvanceTo(ConsultationStatus.InConsultation);
        c.RecordDiagnosis("J45", "Asthma", true);

        // Status is now DiagnosisRecorded, but a direct Complete without diagnosis in a fresh consult:
        var c2 = StartConsultation();
        c2.RecordTriage(TriageData.Create(null, null, null, null, null).Value);

        Assert.Throws<InvalidConsultationTransitionException>(() => c2.AdvanceTo(ConsultationStatus.Completed));
    }

    [Fact]
    public void Illegal_transition_throws()
    {
        var c = StartConsultation();
        Assert.Throws<InvalidConsultationTransitionException>(() => c.AdvanceTo(ConsultationStatus.Completed));
    }

    [Fact]
    public void RecordTriage_only_in_registered_state()
    {
        var c = StartConsultation();
        c.RecordTriage(TriageData.Create(null, null, null, null, null).Value);

        var again = c.RecordTriage(TriageData.Create(null, null, null, null, null).Value);
        Assert.True(again.IsFailure);
    }

    [Fact]
    public void AttachLabOrder_moves_to_awaiting_results()
    {
        var c = StartConsultation();
        c.RecordTriage(TriageData.Create(null, null, null, null, null).Value);
        c.AdvanceTo(ConsultationStatus.AwaitingClinician);
        c.AdvanceTo(ConsultationStatus.InConsultation);

        c.AttachLabOrder(Guid.NewGuid(), "Pending");

        Assert.Equal(ConsultationStatus.AwaitingLabResults, c.Status);
        Assert.Single(c.LabOrders);
    }
}

public class TriageDataTests
{
    [Fact]
    public void Implausible_vitals_fail()
    {
        Assert.True(TriageData.Create(60m, null, null, null, null).IsFailure); // temp > 45
        Assert.True(TriageData.Create(null, null, -1, null, null).IsFailure);  // pulse < 0
    }

    [Fact]
    public void Valid_vitals_succeed()
    {
        Assert.True(TriageData.Create(36.8m, "118/76", 68, 15, 62.5m).IsSuccess);
    }
}
