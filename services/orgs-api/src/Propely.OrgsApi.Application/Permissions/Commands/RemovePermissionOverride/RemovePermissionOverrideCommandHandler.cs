// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Permissions.Interfaces;
using Propely.OrgsApi.Application.Permissions.Services;
using Propely.OrgsApi.Domain.Organizations;

namespace Propely.OrgsApi.Application.Permissions.Commands.RemovePermissionOverride;

public sealed class RemovePermissionOverrideCommandHandler : IRequestHandler<RemovePermissionOverrideCommand>
{
    private readonly IPermissionOverrideRepository _overrideRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CachedPermissionEvaluator _cachedEvaluator;
    private readonly ILogger<RemovePermissionOverrideCommandHandler> _logger;

    public RemovePermissionOverrideCommandHandler(
        IPermissionOverrideRepository overrideRepository,
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork,
        CachedPermissionEvaluator cachedEvaluator,
        ILogger<RemovePermissionOverrideCommandHandler> logger)
    {
        _overrideRepository = overrideRepository;
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
        _cachedEvaluator = cachedEvaluator;
        _logger = logger;
    }

    public async Task Handle(RemovePermissionOverrideCommand request, CancellationToken cancellationToken)
    {
        // Verify requesting user is admin or owner
        var requestingMembership = await _membershipRepository.GetAsync(
            request.OrganizationId, request.RequestingUserId, cancellationToken);

        if (requestingMembership is null || requestingMembership.IsDeleted)
            throw new UnauthorizedAccessException("You do not have access to this organization.");

        if (requestingMembership.Role is not (MembershipRole.Admin or MembershipRole.Owner))
            throw new UnauthorizedAccessException("Only admins and owners can manage permission overrides.");

        // Verify target user is an active member and not an owner
        var targetMembership = await _membershipRepository.GetAsync(
            request.OrganizationId, request.TargetUserId, cancellationToken);

        if (targetMembership is null || targetMembership.IsDeleted)
            throw new InvalidOperationException("Target user is not a member of this organization.");

        if (targetMembership.Role == MembershipRole.Owner)
            throw new InvalidOperationException("Cannot remove permission overrides for owners. Owners always have full access.");

        var existing = await _overrideRepository.GetOverrideAsync(
            request.TargetUserId, request.OrganizationId, request.Permission, cancellationToken);

        if (existing is null)
            return; // No override to remove, silently succeed

        existing.Revoke();
        await _overrideRepository.RemoveAsync(existing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cachedEvaluator.InvalidateAsync(request.TargetUserId, request.OrganizationId, cancellationToken);

        _logger.LogInformation(
            "Permission override removed: {Permission} for user {TargetUserId} in org {OrgId} by {ActorId}",
            request.Permission, request.TargetUserId, request.OrganizationId, request.RequestingUserId);
    }
}
