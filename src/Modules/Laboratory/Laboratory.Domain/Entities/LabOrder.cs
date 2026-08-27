using Jacana.SharedKernel.Domain;

namespace Jacana.Laboratory.Domain;

/// <summary>A laboratory order referencing a consultation by ID.</summary>
public sealed class LabOrder : AggregateRoot<Guid>
{
    private readonly List<LabTestItem> _tests = new();

    private LabOrder() { } // EF

    private LabOrder(Guid id, FacilityId facilityId, Guid patientId, Guid consultationId,
        Guid orderedByUserId, DateTime orderedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        PatientId = patientId;
        ConsultationId = consultationId;
        OrderedByUserId = orderedByUserId;
        OrderedAtUtc = orderedAtUtc;
        Status = LabOrderStatus.Pending;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public Guid ConsultationId { get; private set; }
    public Guid OrderedByUserId { get; private set; }
    public DateTime OrderedAtUtc { get; private set; }
    public LabOrderStatus Status { get; private set; }

    public IReadOnlyCollection<LabTestItem> Tests => _tests.AsReadOnly();

    public static Result<LabOrder> Create(
        Guid id, FacilityId facilityId, Guid patientId, Guid consultationId,
        Guid orderedByUserId, DateTime orderedAtUtc)
    {
        if (patientId == Guid.Empty) return Error.Validation("Patient is required.");
        if (orderedByUserId == Guid.Empty) return Error.Validation("Ordering user is required.");
        return new LabOrder(id, facilityId, patientId, consultationId, orderedByUserId, orderedAtUtc);
    }

    public Result AddTest(string testCode, string testName)
    {
        var test = LabTestItem.Create(testCode, testName);
        if (test.IsFailure) return test.Error;
        _tests.Add(test.Value);
        return Result.Success();
    }

    /// <summary>
    /// Records a result on a test and publishes <see cref="LabResultRecordedDomainEvent"/>
    /// (delivered via the outbox to the Notifications module).
    /// </summary>
    public Result RecordTestResult(Guid testItemId, string? resultValue, string? resultUnit,
        string? referenceRange, bool? isAbnormal, Guid resultedByUserId, DateTime resultedAtUtc)
    {
        var test = _tests.FirstOrDefault(t => t.Id == testItemId);
        if (test is null) return Error.NotFound("Lab test not found.");

        var result = test.RecordResult(resultValue, resultUnit, referenceRange, isAbnormal,
            resultedByUserId, resultedAtUtc);
        if (result.IsFailure) return result.Error;

        RecomputeStatus();
        AddDomainEvent(new LabResultRecordedDomainEvent(Id, PatientId, testItemId, resultedAtUtc));
        return Result.Success();
    }

    private void RecomputeStatus()
    {
        if (_tests.Count > 0 && _tests.All(t => t.Status is LabTestStatus.Resulted or LabTestStatus.Rejected))
            Status = LabOrderStatus.Completed;
        else if (_tests.Any(t => t.Status != LabTestStatus.Ordered))
            Status = LabOrderStatus.PartiallyCompleted;
        else
            Status = LabOrderStatus.Pending;
    }
}
