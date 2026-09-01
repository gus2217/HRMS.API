using Jacana.Notifications.Application.Abstractions;
using Jacana.Notifications.Domain;
using Jacana.Notifications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Notifications.Infrastructure.Repositories;

public sealed class NotificationPreferenceRepository(NotificationsDbContext db) : INotificationPreferenceRepository
{
    public Task<NotificationPreference?> GetAsync(
        Guid recipientUserId, NotificationCategory category, CancellationToken ct = default)
        => db.NotificationPreferences.FirstOrDefaultAsync(
            p => p.RecipientUserId == recipientUserId && p.Category == category, ct);

    public async Task<IReadOnlyList<NotificationPreference>> GetByUserAsync(
        Guid recipientUserId, CancellationToken ct = default)
    {
        var prefs = await db.NotificationPreferences.AsNoTracking()
            .Where(p => p.RecipientUserId == recipientUserId)
            .ToListAsync(ct);
        return prefs;
    }

    public Task AddAsync(NotificationPreference preference, CancellationToken ct = default)
    {
        db.NotificationPreferences.Add(preference);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(NotificationPreference preference, CancellationToken ct = default)
        => Task.CompletedTask; // already tracked from GetAsync

    public async Task<IReadOnlyList<Guid>> FilterInAppEnabledAsync(
        IReadOnlyCollection<Guid> recipientUserIds, NotificationCategory category, CancellationToken ct = default)
    {
        if (recipientUserIds.Count == 0) return recipientUserIds.ToArray();

        var disabled = await db.NotificationPreferences.AsNoTracking()
            .Where(p => p.Category == category
                        && recipientUserIds.Contains(p.RecipientUserId)
                        && !p.InAppEnabled)
            .Select(p => p.RecipientUserId)
            .ToListAsync(ct);

        var disabledSet = disabled.ToHashSet();
        return recipientUserIds.Where(id => !disabledSet.Contains(id)).ToArray();
    }
}
