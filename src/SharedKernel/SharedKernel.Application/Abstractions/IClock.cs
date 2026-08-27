namespace Jacana.SharedKernel.Application.Abstractions;

/// <summary>
/// Abstraction over the system clock. Domain/Application code must never call
/// DateTime.UtcNow directly — inject this instead (testability + determinism).
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
