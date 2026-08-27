using Jacana.SharedKernel.Domain;

namespace Jacana.PatientRegistration.Domain;

/// <summary>A person to contact in an emergency. Owned by the Patient aggregate.</summary>
public sealed class NextOfKin : Entity<Guid>
{
    private NextOfKin() { } // EF

    internal NextOfKin(Guid id, string fullName, string relationship, PhoneNumber phone)
        : base(id)
    {
        FullName = fullName;
        Relationship = relationship;
        Phone = phone;
    }

    public string FullName { get; private set; } = string.Empty;
    public string Relationship { get; private set; } = string.Empty;
    public PhoneNumber Phone { get; private set; } = null!;

    internal static Result<NextOfKin> Create(string fullName, string relationship, PhoneNumber phone)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return Error.Validation("Next of kin full name is required.");
        if (string.IsNullOrWhiteSpace(relationship)) return Error.Validation("Relationship is required.");
        return new NextOfKin(Guid.NewGuid(), fullName.Trim(), relationship.Trim(), phone);
    }
}
