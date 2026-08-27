using Jacana.SharedKernel.Domain;

namespace Jacana.Inventory.Domain;

/// <summary>Immutable record of a stock movement. Never updated or deleted.</summary>
public sealed class StockMovement : Entity<Guid>
{
    private StockMovement() { } // EF

    private StockMovement(Guid id, StockMovementType type, int quantity, string? reference,
        Guid performedByUserId, DateTime atUtc)
        : base(id)
    {
        Type = type;
        Quantity = quantity;
        Reference = reference;
        PerformedByUserId = performedByUserId;
        MovementAtUtc = atUtc;
    }

    public StockMovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public string? Reference { get; private set; }
    public Guid PerformedByUserId { get; private set; }
    public DateTime MovementAtUtc { get; private set; }

    internal static StockMovement Create(StockMovementType type, int quantity, string? reference,
        Guid performedByUserId, DateTime atUtc)
        => new(Guid.NewGuid(), type, quantity, reference, performedByUserId, atUtc);
}
