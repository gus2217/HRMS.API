namespace Jacana.SharedKernel.Domain;

/// <summary>
/// Functional result for expected business-rule failures. Exceptions are reserved
/// for truly exceptional states; anything a user can trigger should come back as
/// <see cref="Result.Failure(Error)"/> instead.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot carry an error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failed result must carry an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value);
    public static Result<T> Failure<T>(Error error) => new(error);

    public static implicit operator Result(Error error) => new(false, error);

    public static Result Combine(params Result[] results)
    {
        foreach (var r in results)
            if (r.IsFailure) return r;
        return Success();
    }
}

public class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T value) : base(true, Error.None) => _value = value;
    internal Result(Error error) : base(false, error) => _value = default;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot read Value from a failed result.");

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);
}
