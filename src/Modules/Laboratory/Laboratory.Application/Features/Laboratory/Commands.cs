using Jacana.Laboratory.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Laboratory.Application.Features.Laboratory;

public sealed record CreateLabOrderCommand(
    Guid PatientId,
    Guid ConsultationId,
    IReadOnlyList<LabTestInput> Tests)
    : ICommand<Result<LabOrderDetailDto>>;

public sealed record LabTestInput(string TestCode, string TestName);

public sealed record RecordLabResultCommand(
    Guid LabOrderId,
    Guid TestItemId,
    string? ResultValue,
    string? ResultUnit,
    string? ReferenceRange,
    bool? IsAbnormal)
    : ICommand<Result<LabOrderDetailDto>>;

public sealed record CancelLabOrderCommand(
    Guid LabOrderId,
    string? Reason)
    : ICommand<Result<LabOrderDetailDto>>;
