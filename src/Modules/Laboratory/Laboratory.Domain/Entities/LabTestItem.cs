using Jacana.SharedKernel.Domain;

namespace Jacana.Laboratory.Domain;

/// <summary>A single test on a lab order.</summary>
public sealed class LabTestItem : Entity<Guid>
{
    private LabTestItem() { } // EF

    internal LabTestItem(Guid id, string testCode, string testName)
        : base(id)
    {
        TestCode = testCode;
        TestName = testName;
        Status = LabTestStatus.Ordered;
    }

    public string TestCode { get; private set; } = string.Empty;
    public string TestName { get; private set; } = string.Empty;
    public LabTestStatus Status { get; private set; }
    public string? ResultValue { get; private set; }
    public string? ResultUnit { get; private set; }
    public string? ReferenceRange { get; private set; }
    public bool? IsAbnormal { get; private set; }
    public Guid? ResultedByUserId { get; private set; }
    public DateTime? ResultedAtUtc { get; private set; }

    internal static Result<LabTestItem> Create(string testCode, string testName)
    {
        if (string.IsNullOrWhiteSpace(testCode)) return Error.Validation("Test code is required.");
        if (string.IsNullOrWhiteSpace(testName)) return Error.Validation("Test name is required.");
        return new LabTestItem(Guid.NewGuid(), testCode.Trim().ToUpperInvariant(), testName.Trim());
    }

    public Result RecordResult(string? resultValue, string? resultUnit, string? referenceRange,
        bool? isAbnormal, Guid resultedByUserId, DateTime resultedAtUtc)
    {
        if (Status is LabTestStatus.Resulted or LabTestStatus.Rejected)
            return Error.InvalidOperation($"Cannot record a result on a test in status {Status}.");

        ResultValue = resultValue;
        ResultUnit = resultUnit;
        ReferenceRange = referenceRange;
        IsAbnormal = isAbnormal;
        ResultedByUserId = resultedByUserId;
        ResultedAtUtc = resultedAtUtc;
        Status = LabTestStatus.Resulted;
        return Result.Success();
    }
}
