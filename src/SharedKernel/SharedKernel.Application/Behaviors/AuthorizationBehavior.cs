using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.SharedKernel.Application.Behaviors;

/// <summary>
/// Authorization is policy-based. A request implementing <see cref="IAuthorizableRequest"/>
/// has its <c>Policy</c> checked against the current user's permissions before the handler runs.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse>(ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is IAuthorizableRequest authorized)
        {
            if (!currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("Authentication required.");

            if (!currentUser.Permissions.Contains(authorized.Policy))
                throw new UnauthorizedAccessException($"Permission '{authorized.Policy}' required.");
        }

        return await next();
    }
}
