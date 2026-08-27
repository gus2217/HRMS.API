using Jacana.SharedKernel.Domain;

namespace Jacana.SharedKernel.Application.Abstractions;

/// <summary>Resolved from the authenticated principal for the current request.</summary>
public interface ICurrentUser
{
    Guid UserId { get; }
    FacilityId FacilityId { get; }
    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<string> Permissions { get; }
    bool IsAuthenticated { get; }
}
