using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Domain;
using Xunit;

namespace Jacana.Tests.Unit.Clinical;

public class FlagsAttachmentsOrdersTests
{
    private static readonly FacilityId Facility = FacilityId.New();
    private static readonly Guid Patient = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    // ── Patient flags ─────────────────────────────────────────

    [Fact]
    public void Flag_requires_message_and_patient()
    {
        Assert.True(PatientFlag.Raise(Facility, Guid.Empty, PatientFlagType.Allergy, "x", User, DateTime.UtcNow).IsFailure);
        Assert.True(PatientFlag.Raise(Facility, Patient, PatientFlagType.Allergy, " ", User, DateTime.UtcNow).IsFailure);
    }

    [Fact]
    public void Flag_starts_active_and_deactivates()
    {
        var f = PatientFlag.Raise(Facility, Patient, PatientFlagType.Allergy, "Penicillin", User, DateTime.UtcNow).Value;
        Assert.True(f.IsActive);

        Assert.True(f.Deactivate(User, DateTime.UtcNow).IsSuccess);
        Assert.False(f.IsActive);
    }

    [Fact]
    public void Flag_cannot_deactivate_twice()
    {
        var f = PatientFlag.Raise(Facility, Patient, PatientFlagType.Warning, "Fall risk", User, DateTime.UtcNow).Value;
        f.Deactivate(User, DateTime.UtcNow);
        Assert.True(f.Deactivate(User, DateTime.UtcNow).IsFailure);
    }

    // ── Attachments ───────────────────────────────────────────

    [Fact]
    public void Attachment_requires_file_name_and_key()
    {
        Assert.True(PatientAttachment.Create(Facility, Patient, " ", "text/plain", 10, "General", "k", User, DateTime.UtcNow).IsFailure);
        Assert.True(PatientAttachment.Create(Facility, Patient, "a.pdf", "text/plain", 10, "General", " ", User, DateTime.UtcNow).IsFailure);
    }

    [Fact]
    public void Attachment_rejects_negative_size()
    {
        Assert.True(PatientAttachment.Create(Facility, Patient, "a.pdf", "text/plain", -1, "General", "k", User, DateTime.UtcNow).IsFailure);
    }

    // ── Diagnostic orders ─────────────────────────────────────

    [Fact]
    public void Order_requires_name_and_indication()
    {
        Assert.True(DiagnosticOrder.Create(Facility, Patient, null, DiagnosticOrderType.Imaging,
            " ", null, "Fever", DiagnosticOrderPriority.Routine, User, DateTime.UtcNow).IsFailure);
        Assert.True(DiagnosticOrder.Create(Facility, Patient, null, DiagnosticOrderType.Imaging,
            "X-ray", null, " ", DiagnosticOrderPriority.Routine, User, DateTime.UtcNow).IsFailure);
    }

    [Fact]
    public void Order_lifecycle_ordered_to_reported()
    {
        var o = DiagnosticOrder.Create(Facility, Patient, null, DiagnosticOrderType.Imaging,
            "Chest X-ray", "Chest", "Cough 2 weeks", DiagnosticOrderPriority.Urgent, User, DateTime.UtcNow).Value;

        Assert.Equal(DiagnosticOrderStatus.Ordered, o.Status);

        Assert.True(o.MarkPerformed(User, DateTime.UtcNow).IsSuccess);
        Assert.Equal(DiagnosticOrderStatus.Performed, o.Status);

        Assert.True(o.RecordReport("No consolidation", User, DateTime.UtcNow).IsSuccess);
        Assert.Equal(DiagnosticOrderStatus.Reported, o.Status);
        Assert.Equal("No consolidation", o.Report);
    }

    [Fact]
    public void Order_cannot_report_before_performed()
    {
        var o = DiagnosticOrder.Create(Facility, Patient, null, DiagnosticOrderType.Imaging,
            "X-ray", null, "Injury", DiagnosticOrderPriority.Routine, User, DateTime.UtcNow).Value;

        Assert.True(o.RecordReport("n/a", User, DateTime.UtcNow).IsFailure);
    }

    [Fact]
    public void Order_cannot_cancel_reported()
    {
        var o = DiagnosticOrder.Create(Facility, Patient, null, DiagnosticOrderType.Procedure,
            "Minor surgery", null, "Abscess", DiagnosticOrderPriority.Routine, User, DateTime.UtcNow).Value;
        o.MarkPerformed(User, DateTime.UtcNow);
        o.RecordReport("Done", User, DateTime.UtcNow);

        Assert.True(o.Cancel("Performed already", User, DateTime.UtcNow).IsFailure);
    }
}
