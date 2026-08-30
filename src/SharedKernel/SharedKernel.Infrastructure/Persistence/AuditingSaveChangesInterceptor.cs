using System.Security.Claims;
using System.Text.Json;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Jacana.SharedKernel.Infrastructure.Persistence;

public enum AuditAction
{
    Create,
    Update,
    Delete,
    SoftDelete,
    Login,
    FailedLogin,
    PermissionDenied
}

/// <summary>
/// Single SaveChanges interceptor that (a) stamps audit fields, (b) converts hard
/// deletes to soft deletes, (c) stamps RowVersion, and (d) emits AuditLogEntry rows
/// for every mutating change. No module writes to AuditLogEntry directly.
/// </summary>
public sealed class AuditingSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor, IClock clock)
    : SaveChangesInterceptor
{
    private Guid CurrentUserId
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;
            var value = principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    private DateTime NowUtc => clock.UtcNow;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null) return;

        var auditEntries = new List<AuditLogEntry>();

        foreach (var entry in context.ChangeTracker.Entries().Where(e =>
                     e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (entry.Entity is AuditLogEntry) continue; // never audit the audit trail

            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is IAuditable)
                    {
                        entry.Property(nameof(IAuditable.CreatedAtUtc)).CurrentValue = NowUtc;
                        entry.Property(nameof(IAuditable.CreatedByUserId)).CurrentValue = CurrentUserId;
                    }
                    StampRowVersion(entry);
                    auditEntries.Add(BuildEntry(entry, AuditAction.Create));
                    break;

                case EntityState.Modified:
                    if (entry.Entity is IAuditable)
                    {
                        entry.Property(nameof(IAuditable.ModifiedAtUtc)).CurrentValue = NowUtc;
                        entry.Property(nameof(IAuditable.ModifiedByUserId)).CurrentValue = CurrentUserId;
                    }
                    StampRowVersion(entry);
                    auditEntries.Add(BuildEntry(entry, AuditAction.Update));
                    break;

                case EntityState.Deleted:
                    if (entry.Entity is ISoftDelete)
                    {
                        // Convert hard delete -> soft delete (global structural rule).
                        entry.State = EntityState.Modified;
                        entry.Property(nameof(ISoftDelete.IsDeleted)).CurrentValue = true;
                        entry.Property(nameof(ISoftDelete.DeletedAtUtc)).CurrentValue = NowUtc;
                        entry.Property(nameof(ISoftDelete.DeletedByUserId)).CurrentValue = CurrentUserId;
                        if (entry.Entity is IAuditable)
                        {
                            entry.Property(nameof(IAuditable.ModifiedAtUtc)).CurrentValue = NowUtc;
                            entry.Property(nameof(IAuditable.ModifiedByUserId)).CurrentValue = CurrentUserId;
                        }
                        auditEntries.Add(BuildEntry(entry, AuditAction.SoftDelete));
                    }
                    else
                    {
                        auditEntries.Add(BuildEntry(entry, AuditAction.Delete));
                    }
                    break;
            }
        }

        if (auditEntries.Count > 0)
            context.Set<AuditLogEntry>().AddRange(auditEntries);
    }

    private static void StampRowVersion(EntityEntry entry)
    {
        if (entry.Entity is AggregateRoot<Guid> aggregate)
            aggregate.StampRowVersion();
    }

    private AuditLogEntry BuildEntry(EntityEntry entry, AuditAction action)
    {
        var entityType = entry.Metadata.ClrType.Name;
        var entityId = entry.Properties
            .FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString()
            ?? "?";

        string? before = null;
        string? after = null;

        if (action is AuditAction.Update or AuditAction.SoftDelete)
        {
            before = SerializeSnapshot(entry.OriginalValues, entry.Metadata);
            after = SerializeSnapshot(entry.CurrentValues, entry.Metadata);
        }
        else if (action == AuditAction.Create)
        {
            after = SerializeSnapshot(entry.CurrentValues, entry.Metadata);
        }

        return new AuditLogEntry
        {
            Id = Guid.NewGuid(),
            FacilityId = Guid.Empty, // resolved by a wrapper for multi-tenant; see note
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            BeforeValuesJson = before,
            AfterValuesJson = after,
            PerformedByUserId = CurrentUserId,
            PerformedAtUtc = NowUtc
        };
    }

    private static string? SerializeSnapshot(
        Microsoft.EntityFrameworkCore.ChangeTracking.PropertyValues values,
        Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in entityType.GetProperties())
        {
            var name = prop.Name;
            var value = values[name];
            // Redact sensitive fields from the audit trail.
            if (name.Contains("Password", StringComparison.OrdinalIgnoreCase)
                || name.Contains("NationalId", StringComparison.OrdinalIgnoreCase)
                || name.Contains("ShaNumber", StringComparison.OrdinalIgnoreCase)
                || name.Contains("InsuranceNumber", StringComparison.OrdinalIgnoreCase)
                || name.Contains("TotpSecret", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Token", StringComparison.OrdinalIgnoreCase))
                dict[name] = "[REDACTED]";
            else
                dict[name] = value;
        }
        return JsonSerializer.Serialize(dict);
    }
}
