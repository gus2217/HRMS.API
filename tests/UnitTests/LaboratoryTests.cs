using Jacana.Laboratory.Domain;
using Jacana.SharedKernel.Domain;
using Xunit;

namespace Jacana.Tests.Unit.Laboratory;

public class LabOrderTests
{
    private static LabOrder NewOrder()
        => LabOrder.Create(Guid.NewGuid(), FacilityId.New(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow).Value;

    [Fact]
    public void Recording_result_publishes_domain_event()
    {
        var order = NewOrder();
        order.AddTest("CBC", "Complete Blood Count");
        var test = order.Tests.Single();

        var result = order.RecordTestResult(test.Id, "5.0", "10^9/L", "4.0-11.0", false,
            Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Single(order.DomainEvents);
        Assert.IsType<LabResultRecordedDomainEvent>(order.DomainEvents.Single());
        Assert.Equal(LabOrderStatus.Completed, order.Status);
    }

    [Fact]
    public void Cannot_record_result_twice()
    {
        var order = NewOrder();
        order.AddTest("CBC", "Complete Blood Count");
        var test = order.Tests.Single();

        order.RecordTestResult(test.Id, "5.0", null, null, null, Guid.NewGuid(), DateTime.UtcNow);
        var second = order.RecordTestResult(test.Id, "6.0", null, null, null, Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(second.IsFailure);
    }

    [Fact]
    public void Partial_completion_when_some_tests_pending()
    {
        var order = NewOrder();
        order.AddTest("CBC", "Complete Blood Count");
        order.AddTest("LFT", "Liver Function Test");
        var first = order.Tests.First();

        order.RecordTestResult(first.Id, "5.0", null, null, null, Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal(LabOrderStatus.PartiallyCompleted, order.Status);
    }
}
