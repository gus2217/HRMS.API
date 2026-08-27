namespace Jacana.SharedKernel.Application.Abstractions;

/// <summary>
/// Commit boundary for a module. The infrastructure implementation commits the
/// module DbContext and dispatches its outbox in one step.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
