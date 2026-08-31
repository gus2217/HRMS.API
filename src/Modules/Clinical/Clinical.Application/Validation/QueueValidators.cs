using FluentValidation;
using Jacana.Clinical.Application.Features.Queue;
using Jacana.Clinical.Domain;

namespace Jacana.Clinical.Application.Validation;

public sealed class CreateQueueEntryCommandValidator : AbstractValidator<CreateQueueEntryCommand>
{
    public CreateQueueEntryCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ClinicType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(v => Enum.TryParse<QueuePriority>(v, true, out _))
            .WithMessage("Priority must be Routine, Urgent or Emergency.");
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class AcceptQueueEntryCommandValidator : AbstractValidator<AcceptQueueEntryCommand>
{
    public AcceptQueueEntryCommandValidator() => RuleFor(x => x.QueueEntryId).NotEmpty();
}

public sealed class CancelQueueEntryCommandValidator : AbstractValidator<CancelQueueEntryCommand>
{
    public CancelQueueEntryCommandValidator() => RuleFor(x => x.QueueEntryId).NotEmpty();
}

public sealed class SearchQueueEntriesQueryValidator : AbstractValidator<SearchQueueEntriesQuery>
{
    public SearchQueueEntriesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.ClinicType).MaximumLength(64);
        RuleFor(x => x.Status)
            .Must(v => string.IsNullOrWhiteSpace(v) || Enum.TryParse<QueueStatus>(v, true, out _))
            .WithMessage("Status must be Waiting, Accepted, Completed or Cancelled.");
    }
}
