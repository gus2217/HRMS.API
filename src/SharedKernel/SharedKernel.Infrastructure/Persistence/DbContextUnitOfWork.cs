using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Jacana.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Generic unit-of-work over a module DbContext. Committing also triggers the
/// SaveChanges interceptors (audit + soft delete + outbox) atomically.
/// </summary>
public sealed class DbContextUnitOfWork<TContext>(TContext dbContext) : IUnitOfWork
    where TContext : DbContext
{
    public bool HasChanges => dbContext.ChangeTracker.HasChanges();

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => dbContext.SaveChangesAsync(ct);
}
