using Jacana.Identity.Application.Abstractions;
using Jacana.Identity.Application.DTOs;
using Jacana.Identity.Application.Features.Users;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Identity.Application.Features.Users.List;

/// <summary>
/// Paged staff directory. Search matches name, email or phone (case-insensitive).
/// Read side only — repository returns the projected page (AsNoTracking).
/// </summary>
public sealed class GetUsersQueryHandler(IUserRepository users)
    : IRequestHandler<GetUsersQuery, Result<PagedResult<StaffUserListItemDto>>>
{
    public async Task<Result<PagedResult<StaffUserListItemDto>>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var page = await users.GetPageAsync(request.PageNumber, request.PageSize, request.Search, ct);
        return Result.Success(page);
    }
}

public sealed class GetUserDetailQueryHandler(IUserRepository users)
    : IRequestHandler<GetUserDetailQuery, Result<StaffUserDetailDto>>
{
    public async Task<Result<StaffUserDetailDto>> Handle(GetUserDetailQuery request, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(request.UserId, ct);
        if (user is null) return Error.NotFound("User not found.");

        var effective = await users.GetPermissionCodesAsync(user.Id, ct);
        return StaffUserMapping.ToDetail(user, effective);
    }
}
