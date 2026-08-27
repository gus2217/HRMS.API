using Jacana.Laboratory.Application.Abstractions;
using Jacana.Laboratory.Application.DTOs;
using Jacana.Laboratory.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Laboratory.Application.Features.Laboratory.Handlers;

public sealed class CreateLabOrderCommandHandler(
    ILabOrderRepository orders,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<CreateLabOrderCommand, Result<LabOrderDetailDto>>
{
    public async Task<Result<LabOrderDetailDto>> Handle(CreateLabOrderCommand request, CancellationToken ct)
    {
        var order = LabOrder.Create(
            Guid.NewGuid(), currentUser.FacilityId, request.PatientId, request.ConsultationId,
            currentUser.UserId, clock.UtcNow);
        if (order.IsFailure) return order.Error;

        foreach (var test in request.Tests)
        {
            var add = order.Value.AddTest(test.TestCode, test.TestName);
            if (add.IsFailure) return add.Error;
        }

        await orders.AddAsync(order.Value, ct);
        // Map from the in-memory aggregate — the unit-of-work transaction has not
        // committed yet, so a re-query would not see the new row.
        return LabOrderMapper.ToDetail(order.Value);
    }
}

public sealed class RecordLabResultCommandHandler(
    ILabOrderRepository orders,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<RecordLabResultCommand, Result<LabOrderDetailDto>>
{
    public async Task<Result<LabOrderDetailDto>> Handle(RecordLabResultCommand request, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(request.LabOrderId, ct);
        if (order is null) return Error.NotFound("Lab order not found.");

        var result = order.RecordTestResult(
            request.TestItemId, request.ResultValue, request.ResultUnit,
            request.ReferenceRange, request.IsAbnormal, currentUser.UserId, clock.UtcNow);
        if (result.IsFailure) return result.Error;

        await orders.UpdateAsync(order, ct);
        return LabOrderMapper.ToDetail(order);
    }
}

public sealed class GetLabOrderQueryHandler(ILabOrderRepository orders)
    : IRequestHandler<GetLabOrderQuery, Result<LabOrderDetailDto>>
{
    public async Task<Result<LabOrderDetailDto>> Handle(GetLabOrderQuery request, CancellationToken ct)
    {
        var detail = await orders.GetDetailAsync(request.LabOrderId, ct);
        return detail is null ? Error.NotFound("Lab order not found.") : detail;
    }
}
