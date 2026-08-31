using Jacana.Pharmacy.Domain;
using Jacana.SharedKernel.Domain;
using Xunit;

namespace Jacana.Tests.Unit.Pharmacy;

public class PrescriptionTests
{
    private static Prescription NewPrescription()
        => Prescription.Create(Guid.NewGuid(), FacilityId.New(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow).Value;

    [Fact]
    public void Dispense_respects_prescribed_quantity()
    {
        var p = NewPrescription();
        p.AddItem(Guid.NewGuid(), "Take 2x daily", "Oral", "Twice daily", 5, 10);
        var item = p.Items.Single();

        Assert.True(p.DispenseItem(item.Id, 6).IsSuccess);
        Assert.Equal(6, item.QuantityDispensed);
        Assert.Equal(PrescriptionItemStatus.PartiallyDispensed, item.Status);
    }

    [Fact]
    public void Cannot_dispense_beyond_prescribed()
    {
        var p = NewPrescription();
        p.AddItem(Guid.NewGuid(), "Take 1x daily", "Oral", "Once daily", 5, 5);
        var item = p.Items.Single();

        p.DispenseItem(item.Id, 5);
        Assert.True(p.DispenseItem(item.Id, 1).IsFailure);
    }

    [Fact]
    public void Fully_dispensed_when_all_items_dispensed()
    {
        var p = NewPrescription();
        p.AddItem(Guid.NewGuid(), "Take 1x daily", "Oral", "Once daily", 5, 3);
        var item = p.Items.Single();

        p.DispenseItem(item.Id, 3);

        Assert.Equal(PrescriptionStatus.FullyDispensed, p.Status);
    }
}

public class DispenseRecordTests
{
    [Fact]
    public void Requires_positive_quantity()
    {
        Assert.True(DispenseRecord.Create(Guid.NewGuid(), FacilityId.New(), Guid.NewGuid(), 0,
            Guid.NewGuid(), DateTime.UtcNow).IsFailure);
    }
}
