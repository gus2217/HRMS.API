using Jacana.SharedKernel.Domain;

namespace Jacana.PatientRegistration.Domain;

/// <summary>A patient's consent to a specific type of data/clinical use.</summary>
public sealed class ConsentRecord : Entity<Guid>
{
    private ConsentRecord() { } // EF

    internal ConsentRecord(Guid id, ConsentType type, bool granted, Guid recordedByUserId, DateTime recordedAtUtc)
        : base(id)
    {
        Type = type;
        Granted = granted;
        RecordedByUserId = recordedByUserId;
        RecordedAtUtc = recordedAtUtc;
    }

    public ConsentType Type { get; private set; }
    public bool Granted { get; private set; }
    public Guid RecordedByUserId { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }

    internal static ConsentRecord Create(ConsentType type, bool granted, Guid recordedByUserId, DateTime recordedAtUtc)
        => new(Guid.NewGuid(), type, granted, recordedByUserId, recordedAtUtc);
}
