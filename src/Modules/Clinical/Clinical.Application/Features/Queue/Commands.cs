using Jacana.Clinical.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Application.Features.Queue;

public sealed record CreateQueueEntryCommand(
    Guid PatientId,
    string ClinicType,
    string Priority,
    string? Notes)
    : ICommand<Result<QueueEntryDto>>;

public sealed record AcceptQueueEntryCommand(
    Guid QueueEntryId)
    : ICommand<Result<AcceptQueueEntryResponseDto>>;

public sealed record CancelQueueEntryCommand(
    Guid QueueEntryId)
    : ICommand<Result<QueueEntryDto>>;

public sealed record SearchQueueEntriesQuery(
    string? ClinicType,
    string? Status,
    int PageNumber,
    int PageSize)
    : IQuery<Result<PagedResult<QueueEntryDto>>>;
