namespace Jacana.SharedKernel.Domain;

/// <summary>Inclusive date range. Guards Start &lt;= End.</summary>
public sealed class DateRange : ValueObject
{
    private DateRange(DateOnly start, DateOnly end)
    {
        Start = start;
        End = end;
    }

    public DateOnly Start { get; }
    public DateOnly End { get; }

    public static Result<DateRange> Create(DateOnly start, DateOnly end)
        => start > end
            ? Error.Validation("Start date cannot be after end date.")
            : new DateRange(start, end);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
}
