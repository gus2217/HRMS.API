namespace Jacana.SharedKernel.Application.Common;

/// <summary>Queries implementing this are eligible for CachingBehavior.</summary>
public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan Expiration { get; }
}

/// <summary>Requests carrying an explicit authorization policy checked by AuthorizationBehavior.</summary>
public interface IAuthorizableRequest
{
    string Policy { get; }
}

/// <summary>Marks a request whose handler must run inside a transaction.</summary>
public interface ITransactionalRequest { }
