using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using MediatR;

namespace Jacana.SharedKernel.Application.Behaviors;

/// <summary>
/// Wraps command handlers in a unit-of-work commit. Queries are excluded
/// (AsNoTracking reads need no transaction).
///
/// Because every module registers its own <see cref="IUnitOfWork"/>, we resolve
/// the full set and commit only the DbContext(s) that actually have tracked
/// changes — the handler's own module context, not the last-registered one.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse>(IEnumerable<IUnitOfWork> unitsOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is not ICommand<TResponse>)
            return await next();

        var response = await next();

        foreach (var unitOfWork in unitsOfWork)
        {
            if (unitOfWork.HasChanges)
                await unitOfWork.SaveChangesAsync(ct);
        }

        return response;
    }
}
