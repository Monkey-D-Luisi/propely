// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Permissions.DTOs;
using Propely.OrgsApi.Application.Permissions.Interfaces;
using Propely.OrgsApi.Domain.Organizations;

namespace Propely.OrgsApi.Application.Permissions.Queries.GetUserPermissions;

public sealed class GetUserPermissionsQueryHandler
    : IRequestHandler<GetUserPermissionsQuery, IReadOnlyList<EffectivePermissionDto>>
{
    private readonly IPermissionEvaluator _permissionEvaluator;
    private readonly IMembershipRepository _membershipRepository;

    public GetUserPermissionsQueryHandler(
        IPermissionEvaluator permissionEvaluator,
        IMembershipRepository membershipRepository)
    {
        _permissionEvaluator = permissionEvaluator;
        _membershipRepository = membershipRepository;
    }

    public async Task<IReadOnlyList<EffectivePermissionDto>> Handle(
        GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        // Verify requesting user has access to view permissions
        var requestingMembership = await _membershipRepository.GetAsync(
            request.OrganizationId, request.RequestingUserId, cancellationToken);

        if (requestingMembership is null || requestingMembership.IsDeleted)
            throw new UnauthorizedAccessException("You do not have access to this organization.");

        // Only admins, owners, or the user themselves can view permissions
        var isSelf = request.RequestingUserId == request.TargetUserId;
        var isAdminOrOwner = requestingMembership.Role is MembershipRole.Admin or MembershipRole.Owner;

        if (!isSelf && !isAdminOrOwner)
            throw new UnauthorizedAccessException("Only admins and owners can view other users' permissions.");

        return await _permissionEvaluator.GetEffectivePermissionsAsync(
            request.TargetUserId, request.OrganizationId, cancellationToken);
    }
}
