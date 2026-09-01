using Jacana.Clinical.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Application.Features.PatientClinical;

public sealed record GetActiveFlagsQuery(Guid PatientId)
    : IQuery<Result<IReadOnlyList<PatientFlagDto>>>;

public sealed record GetAllFlagsQuery(Guid PatientId)
    : IQuery<Result<IReadOnlyList<PatientFlagDto>>>;

public sealed record GetAttachmentsQuery(Guid PatientId)
    : IQuery<Result<IReadOnlyList<PatientAttachmentDto>>>;

public sealed record GetDiagnosticOrdersByPatientQuery(Guid PatientId)
    : IQuery<Result<IReadOnlyList<DiagnosticOrderDto>>>;

public sealed record GetDiagnosticOrdersByConsultationQuery(Guid ConsultationId)
    : IQuery<Result<IReadOnlyList<DiagnosticOrderDto>>>;
