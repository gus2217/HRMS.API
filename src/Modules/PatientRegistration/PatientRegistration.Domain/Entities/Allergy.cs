using Jacana.SharedKernel.Domain;

namespace Jacana.PatientRegistration.Domain;

public sealed class Allergy : Entity<Guid>
{
    private Allergy() { } // EF

    internal Allergy(Guid id, string substance, AllergySeverity severity, string? notes)
        : base(id)
    {
        Substance = substance;
        Severity = severity;
        Notes = notes;
    }

    public string Substance { get; private set; } = string.Empty;
    public AllergySeverity Severity { get; private set; }
    public string? Notes { get; private set; }

    internal static Result<Allergy> Create(string substance, AllergySeverity severity, string? notes)
    {
        if (string.IsNullOrWhiteSpace(substance)) return Error.Validation("Allergy substance is required.");
        return new Allergy(Guid.NewGuid(), substance.Trim(), severity, notes);
    }
}
