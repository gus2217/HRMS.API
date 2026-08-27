using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// Reference to a Pharmacy prescription — ID only. No join to Pharmacy tables.
/// </summary>
public sealed class PrescriptionOrder : Entity<Guid>
{
    private PrescriptionOrder() { } // EF

    internal PrescriptionOrder(Guid id, Guid prescriptionId)
        : base(id)
    {
        PrescriptionId = prescriptionId;
    }

    public Guid PrescriptionId { get; private set; }

    internal static PrescriptionOrder Create(Guid prescriptionId)
        => new(Guid.NewGuid(), prescriptionId);
}
