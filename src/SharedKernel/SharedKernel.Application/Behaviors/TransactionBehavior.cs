using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using MediatR;

namespace Jacana.SharedKernel.Application.Behaviors;

/// <summary>
/// Wraps command handlers in a unit-of-work commit. Queries are excluded
/// (AsNoTracking reads need no transaction).
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is not ICommand<TResponse>)
            return await next();

        var response = await next();
        await unitOfWork.SaveChangesAsync(ct);
        return response;
    }
}
