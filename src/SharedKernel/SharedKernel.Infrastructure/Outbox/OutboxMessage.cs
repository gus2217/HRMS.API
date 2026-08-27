namespace Jacana.SharedKernel.Infrastructure.Outbox;

/// <summary>
/// Outbox message. Written in the same transaction as the domain write, then delivered
/// by the Hangfire dispatcher with retry — guarantees at-least-once side effects.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
}
