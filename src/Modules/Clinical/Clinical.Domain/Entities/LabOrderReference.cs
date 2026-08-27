using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// Snapshot reference to a Laboratory order — ID + status only. The Clinical module
/// never joins to Laboratory tables; this is a denormalized read-cache of the order's
/// status, updated via domain events.
/// </summary>
public sealed class LabOrderReference : Entity<Guid>
{
    private LabOrderReference() { } // EF

    internal LabOrderReference(Guid id, Guid labOrderId, string statusSnapshot)
        : base(id)
    {
        LabOrderId = labOrderId;
        StatusSnapshot = statusSnapshot;
    }

    public Guid LabOrderId { get; private set; }
    public string StatusSnapshot { get; private set; } = string.Empty;

    internal static LabOrderReference Create(Guid labOrderId, string statusSnapshot)
        => new(Guid.NewGuid(), labOrderId, statusSnapshot);
}
