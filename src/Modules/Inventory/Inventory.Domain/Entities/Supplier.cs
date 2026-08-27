using Jacana.SharedKernel.Domain;

namespace Jacana.Inventory.Domain;

public sealed class Supplier : AggregateRoot<Guid>
{
    private Supplier() { } // EF

    private Supplier(Guid id, FacilityId facilityId, string name, PhoneNumber phone, string? email)
        : base(id)
    {
        FacilityId = facilityId;
        Name = name;
        Phone = phone;
        Email = email;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public PhoneNumber Phone { get; private set; } = null!;
    public string? Email { get; private set; }

    public static Result<Supplier> Create(Guid id, FacilityId facilityId, string name, PhoneNumber phone, string? email)
    {
        if (string.IsNullOrWhiteSpace(name)) return Error.Validation("Supplier name is required.");
        return new Supplier(id, facilityId, name.Trim(), phone, email);
    }
}
