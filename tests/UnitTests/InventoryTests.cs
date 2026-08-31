using Jacana.Inventory.Domain;
using Jacana.SharedKernel.Domain;
using Xunit;

namespace Jacana.Tests.Unit.Inventory;

public class DrugTests
{
    [Fact]
    public void Create_requires_code_name_form()
    {
        var price = Money.Create(10m).Value;
        var facility = FacilityId.New();
        Assert.True(Drug.Create(Guid.NewGuid(), facility, "", "Paracetamol", "Analgesic", "Tablet", price, 10).IsFailure);
        Assert.True(Drug.Create(Guid.NewGuid(), facility, "PCM", "", "Analgesic", "Tablet", price, 10).IsFailure);
        Assert.True(Drug.Create(Guid.NewGuid(), facility, "PCM", "Paracetamol", "", "Tablet", price, 10).IsFailure);
        Assert.True(Drug.Create(Guid.NewGuid(), facility, "PCM", "Paracetamol", "Analgesic", "", price, 10).IsFailure);
    }
}

public class StockBatchTests
{
    private static StockBatch NewBatch(int quantity = 100)
        => StockBatch.Receive(Guid.NewGuid(), FacilityId.New(), Guid.NewGuid(), "BATCH-1",
            quantity, new DateOnly(2027, 1, 1), Money.Create(5m).Value, Guid.NewGuid(), DateTime.UtcNow).Value;

    [Fact]
    public void Dispense_reduces_quantity_and_records_movement()
    {
        var batch = NewBatch();
        var result = batch.Dispense(30, "prescription-item-1", Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(70, batch.QuantityOnHand);
        Assert.Equal(2, batch.Movements.Count);
    }

    [Fact]
    public void Cannot_dispense_more_than_on_hand()
    {
        var batch = NewBatch(10);
        Assert.True(batch.Dispense(11, null, Guid.NewGuid(), DateTime.UtcNow).IsFailure);
    }

    [Fact]
    public void Adjust_sets_quantity()
    {
        var batch = NewBatch();
        batch.Adjust(25, Guid.NewGuid(), DateTime.UtcNow);
        Assert.Equal(25, batch.QuantityOnHand);
    }
}

public class MoneyTests2
{
    [Fact]
    public void Zero_and_negative_guards()
    {
        Assert.True(Money.Create(-5m).IsFailure);
        Assert.Equal(0m, Money.Zero().Amount);
    }
}
