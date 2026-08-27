using Jacana.SharedKernel.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jacana.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Generic unit-of-work over a module DbContext. Committing also triggers the
/// SaveChanges interceptors (audit + soft delete + outbox) atomically.
/// </summary>
public sealed class DbContextUnitOfWork<TContext>(TContext dbContext) : IUnitOfWork
    where TContext : DbContext
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => dbContext.SaveChangesAsync(ct);
}
