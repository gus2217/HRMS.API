using Jacana.Laboratory.Application.DTOs;
using Jacana.Laboratory.Domain;

namespace Jacana.Laboratory.Application.Features.Laboratory;

/// <summary>
/// Maps an in-memory <see cref="LabOrder"/> aggregate to its detail DTO.
/// Handlers use this after mutation instead of re-querying the database (the
/// unit-of-work transaction has not committed yet at that point).
/// </summary>
internal static class LabOrderMapper
{
    public static LabOrderDetailDto ToDetail(LabOrder o) =>
        new(
            o.Id, o.PatientId, o.ConsultationId, o.OrderedByUserId,
            o.Status.ToString(), o.OrderedAtUtc,
            o.Tests.Select(t => new LabTestItemDto(
                t.Id, t.TestCode, t.TestName, t.Status.ToString(),
                t.ResultValue, t.ResultUnit, t.ReferenceRange, t.IsAbnormal)).ToArray());
}
