using FluentValidation;
using Jacana.Clinical.Application.Features.Appointments;
using Jacana.Clinical.Domain;

namespace Jacana.Clinical.Application.Validation;

public sealed class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ClinicType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(v => Enum.TryParse<AppointmentType>(v, true, out _))
            .WithMessage("Type must be a valid appointment type.");
        RuleFor(x => x.ScheduledAtUtc).NotEmpty();
        RuleFor(x => x.DurationMinutes).InclusiveBetween(1, 480);
        RuleFor(x => x.Reason).MaximumLength(500);
        RuleFor(x => x.RecurrenceCount).InclusiveBetween(1, 52);
        RuleFor(x => x.RecurrencePattern)
            .Must(v => string.IsNullOrWhiteSpace(v) || Enum.TryParse<RecurrencePattern>(v, true, out _))
            .WithMessage("Recurrence must be None, Daily, Weekly or Monthly.");
    }
}

public sealed class StartAppointmentCommandValidator : AbstractValidator<StartAppointmentCommand>
{
    public StartAppointmentCommandValidator() => RuleFor(x => x.AppointmentId).NotEmpty();
}

public sealed class CompleteAppointmentCommandValidator : AbstractValidator<CompleteAppointmentCommand>
{
    public CompleteAppointmentCommandValidator() => RuleFor(x => x.AppointmentId).NotEmpty();
}

public sealed class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator() => RuleFor(x => x.AppointmentId).NotEmpty();
}

public sealed class NoShowAppointmentCommandValidator : AbstractValidator<NoShowAppointmentCommand>
{
    public NoShowAppointmentCommandValidator() => RuleFor(x => x.AppointmentId).NotEmpty();
}

public sealed class SearchAppointmentsQueryValidator : AbstractValidator<SearchAppointmentsQuery>
{
    public SearchAppointmentsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
        RuleFor(x => x.ClinicType).MaximumLength(64);
    }
}

public sealed class CreateAppointmentRequestCommandValidator : AbstractValidator<CreateAppointmentRequestCommand>
{
    public CreateAppointmentRequestCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ClinicType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class ApproveAppointmentRequestCommandValidator : AbstractValidator<ApproveAppointmentRequestCommand>
{
    public ApproveAppointmentRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.ScheduledAtUtc).NotEmpty();
        RuleFor(x => x.DurationMinutes).InclusiveBetween(1, 480);
        RuleFor(x => x.Type).NotEmpty();
    }
}

public sealed class DeclineAppointmentRequestCommandValidator : AbstractValidator<DeclineAppointmentRequestCommand>
{
    public DeclineAppointmentRequestCommandValidator() => RuleFor(x => x.RequestId).NotEmpty();
}

public sealed class SearchAppointmentRequestsQueryValidator : AbstractValidator<SearchAppointmentRequestsQuery>
{
    public SearchAppointmentRequestsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.ClinicType).MaximumLength(64);
    }
}
