namespace Jacana.SharedKernel.Domain;

public enum Currency
{
    Kes = 0
}

/// <summary>
/// Monetary value. Arithmetic guards against mixing currencies. Stored as a decimal
/// amount plus a currency discriminator (default KES).
/// </summary>
public sealed class Money : ValueObject
{
    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }
    public Currency Currency { get; }

    public static Money Zero(Currency currency = Currency.Kes) => new(0m, currency);

    public static Result<Money> Create(decimal amount, Currency currency = Currency.Kes)
    {
        if (amount < 0) return Error.Validation("Amount cannot be negative.");
        if (amount != decimal.Round(amount, 2)) return Error.Validation("Amount must have at most 2 decimal places.");
        return new Money(amount, currency);
    }

    public static Result<Money> operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            return Error.InvalidOperation($"Cannot combine {a.Currency} with {b.Currency}.");
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Result<Money> operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            return Error.InvalidOperation($"Cannot combine {a.Currency} with {b.Currency}.");
        if (a.Amount < b.Amount)
            return Error.InvalidOperation("Insufficient amount.");
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Result<Money> operator *(Money a, int multiplier)
    {
        if (multiplier < 0)
            return Error.InvalidOperation("Multiplier cannot be negative.");
        return new Money(a.Amount * multiplier, a.Currency);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Currency} {Amount:F2}";
}
