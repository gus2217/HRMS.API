namespace Jacana.SharedKernel.Application.Abstractions;

public sealed record UserIdentityDto(Guid UserId, string FullName);

/// <summary>
/// Resolves display names for users (clinicians/receptionists) referenced by
/// other modules — e.g. the requester/approver on an appointment request.
/// </summary>
public interface IUserIdentityLookup
{
    Task<IReadOnlyDictionary<Guid, UserIdentityDto>> GetIdentitiesAsync(
        IReadOnlyCollection<Guid> userIds, CancellationToken ct = default);
}
