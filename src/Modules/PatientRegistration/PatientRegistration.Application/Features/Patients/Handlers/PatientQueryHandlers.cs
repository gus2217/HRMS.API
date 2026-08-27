using Jacana.PatientRegistration.Application.Abstractions;
using Jacana.PatientRegistration.Application.DTOs;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.PatientRegistration.Application.Features.Patients;

public sealed class GetPatientQueryHandler(IPatientRepository patients)
    : IRequestHandler<GetPatientQuery, Result<PatientDetailDto>>
{
    public async Task<Result<PatientDetailDto>> Handle(GetPatientQuery request, CancellationToken ct)
    {
        var detail = await patients.GetDetailAsync(request.PatientId, ct);
        return detail is null ? Error.NotFound("Patient not found.") : detail;
    }
}

public sealed class SearchPatientsQueryHandler(IPatientRepository patients)
    : IRequestHandler<SearchPatientsQuery, Result<PagedResult<PatientSummaryDto>>>
{
    public async Task<Result<PagedResult<PatientSummaryDto>>> Handle(SearchPatientsQuery request, CancellationToken ct)
    {
        var items = await patients.SearchAsync(request.Search, request.PageNumber, request.PageSize, ct);
        var total = await patients.CountAsync(request.Search, ct);
        return new PagedResult<PatientSummaryDto>(items, total, request.PageNumber, request.PageSize);
    }
}
