using Jacana.SharedKernel.Domain;
using Xunit;

namespace Jacana.Tests.Unit.SharedKernel;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("+254712345678")]
    [InlineData("0712345678")]
    [InlineData("254712345678")]
    public void Valid_kenyan_numbers_are_normalized(string input)
    {
        var result = PhoneNumber.Create(input);
        Assert.True(result.IsSuccess);
        Assert.Equal("+254712345678", result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("+15551234567")]
    [InlineData("07123")]
    public void Invalid_numbers_fail(string input)
    {
        Assert.True(PhoneNumber.Create(input).IsFailure);
    }
}

public class MoneyTests
{
    [Fact]
    public void Adding_same_currency_succeeds()
    {
        var a = Money.Create(10.50m).Value;
        var b = Money.Create(5.25m).Value;
        var sum = a + b;
        Assert.True(sum.IsSuccess);
        Assert.Equal(15.75m, sum.Value.Amount);
    }

    [Fact]
    public void Subtracting_more_than_available_fails()
    {
        var a = Money.Create(5m).Value;
        var b = Money.Create(10m).Value;
        Assert.True((a - b).IsFailure);
    }

    [Fact]
    public void Negative_amount_fails()
    {
        Assert.True(Money.Create(-1m).IsFailure);
    }
}

public class DateRangeTests
{
    [Fact]
    public void Start_after_end_fails()
    {
        Assert.True(DateRange.Create(new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1)).IsFailure);
    }
}

public class ResultTests
{
    [Fact]
    public void Failed_result_requires_error()
    {
        Assert.Throws<InvalidOperationException>(() => Result.Failure(Error.None));
    }

    [Fact]
    public void Success_carries_value()
    {
        Result<int> r = 42;
        Assert.True(r.IsSuccess);
        Assert.Equal(42, r.Value);
    }
}
