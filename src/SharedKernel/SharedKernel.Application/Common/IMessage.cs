namespace Jacana.SharedKernel.Application;

/// <summary>Marker: a MediatR request that performs a read. Excluded from transactions.</summary>
public interface IQuery<out TResponse> : MediatR.IRequest<TResponse> { }

/// <summary>Marker: a MediatR request that mutates state. Wrapped in a transaction.</summary>
public interface ICommand<out TResponse> : MediatR.IRequest<TResponse> { }
