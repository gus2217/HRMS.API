namespace Jacana.SharedKernel.Domain;

public static class ErrorCodes
{
    public const string Validation = "validation.invalid";
    public const string NotFound = "not_found";
    public const string Conflict = "conflict";
    public const string Forbidden = "forbidden";
    public const string Unauthorized = "unauthorized";
    public const string InvalidOperation = "invalid_operation";
    public const string IdempotencyConflict = "idempotency.conflict";
}

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error Validation(string message) => new(ErrorCodes.Validation, message);
    public static Error NotFound(string message) => new(ErrorCodes.NotFound, message);
    public static Error Conflict(string message) => new(ErrorCodes.Conflict, message);
    public static Error Forbidden(string message) => new(ErrorCodes.Forbidden, message);
    public static Error Unauthorized(string message) => new(ErrorCodes.Unauthorized, message);
    public static Error InvalidOperation(string message) => new(ErrorCodes.InvalidOperation, message);
}
