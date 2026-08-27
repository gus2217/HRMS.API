using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

public sealed class Diagnosis : Entity<Guid>
{
    private Diagnosis() { } // EF

    internal Diagnosis(Guid id, string icdCode, string description, bool isPrimary)
        : base(id)
    {
        IcdCode = icdCode;
        Description = description;
        IsPrimary = isPrimary;
    }

    public string IcdCode { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }

    internal static Result<Diagnosis> Create(string icdCode, string description, bool isPrimary)
    {
        if (string.IsNullOrWhiteSpace(icdCode)) return Error.Validation("ICD code is required.");
        if (string.IsNullOrWhiteSpace(description)) return Error.Validation("Diagnosis description is required.");
        return new Diagnosis(Guid.NewGuid(), icdCode.Trim().ToUpperInvariant(), description.Trim(), isPrimary);
    }
}
