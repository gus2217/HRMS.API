using Jacana.Laboratory.Application.Abstractions;
using Jacana.Laboratory.Application.DTOs;
using Jacana.Laboratory.Domain;
using Jacana.Laboratory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Laboratory.Infrastructure.Repositories;

public sealed class LabOrderRepository(LaboratoryDbContext db) : ILabOrderRepository
{
    public async Task<LabOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.LabOrders.Include(o => o.Tests).FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task AddAsync(LabOrder order, CancellationToken ct = default)
    {
        db.LabOrders.Add(order);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(LabOrder order, CancellationToken ct = default)
    {
        db.Entry(order).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public async Task<LabOrderDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var o = await db.LabOrders.AsNoTracking()
            .Include(x => x.Tests)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (o is null) return null;

        return new LabOrderDetailDto(
            o.Id, o.PatientId, o.ConsultationId, o.OrderedByUserId,
            o.Status.ToString(), o.OrderedAtUtc,
            o.Tests.Select(t => new LabTestItemDto(
                t.Id, t.TestCode, t.TestName, t.Status.ToString(),
                t.ResultValue, t.ResultUnit, t.ReferenceRange, t.IsAbnormal)).ToArray());
    }
}
