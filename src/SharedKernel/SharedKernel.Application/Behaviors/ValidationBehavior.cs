using Jacana.SharedKernel.Application.Abstractions;
using MediatR;

namespace Jacana.SharedKernel.Application.Behaviors;

/// <summary>Validates every request with its FluentValidation validators before the handler runs.</summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<FluentValidation.IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var context = new FluentValidation.ValidationContext<TRequest>(request);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .ToList();

        if (failures.Count > 0)
            throw new FluentValidation.ValidationException(failures);

        return await next();
    }
}
