// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Permissions.Interfaces;
using Propely.OrgsApi.Application.Permissions.Services;
using Propely.OrgsApi.Domain.Organizations;
using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.Application.Permissions.Commands.SetPermissionOverride;

public sealed class SetPermissionOverrideCommandHandler : IRequestHandler<SetPermissionOverrideCommand>
{
    private readonly IPermissionOverrideRepository _overrideRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CachedPermissionEvaluator _cachedEvaluator;
    private readonly ILogger<SetPermissionOverrideCommandHandler> _logger;

    public SetPermissionOverrideCommandHandler(
        IPermissionOverrideRepository overrideRepository,
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork,
        CachedPermissionEvaluator cachedEvaluator,
        ILogger<SetPermissionOverrideCommandHandler> logger)
    {
        _overrideRepository = overrideRepository;
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
        _cachedEvaluator = cachedEvaluator;
        _logger = logger;
    }

    public async Task Handle(SetPermissionOverrideCommand request, CancellationToken cancellationToken)
    {
        // Verify requesting user is admin or owner in the organization
        var requestingMembership = await _membershipRepository.GetAsync(
            request.OrganizationId, request.RequestingUserId, cancellationToken);

        if (requestingMembership is null || requestingMembership.IsDeleted)
            throw new UnauthorizedAccessException("You do not have access to this organization.");

        if (requestingMembership.Role is not (MembershipRole.Admin or MembershipRole.Owner))
            throw new UnauthorizedAccessException("Only admins and owners can manage permission overrides.");

        // Cannot modify owner permissions
        var targetMembership = await _membershipRepository.GetAsync(
            request.OrganizationId, request.TargetUserId, cancellationToken);

        if (targetMembership is null || targetMembership.IsDeleted)
            throw new InvalidOperationException("Target user is not a member of this organization.");

        if (targetMembership.Role == MembershipRole.Owner)
            throw new InvalidOperationException("Cannot set permission overrides for owners. Owners always have full access.");

        // Check for existing override
        var existing = await _overrideRepository.GetOverrideAsync(
            request.TargetUserId, request.OrganizationId, request.Permission, cancellationToken);

        if (existing is not null)
        {
            // Remove old override and create new one (since Granted is immutable)
            existing.Revoke();
            await _overrideRepository.RemoveAsync(existing, cancellationToken);
        }

        var newOverride = PermissionOverride.Create(
            request.TargetUserId,
            request.OrganizationId,
            request.Permission,
            request.Granted,
            request.RequestingUserId);

        await _overrideRepository.AddAsync(newOverride, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cachedEvaluator.InvalidateAsync(request.TargetUserId, request.OrganizationId, cancellationToken);

        _logger.LogInformation(
            "Permission override set: {Permission} = {Granted} for user {TargetUserId} in org {OrgId} by {ActorId}",
            request.Permission, request.Granted, request.TargetUserId, request.OrganizationId, request.RequestingUserId);
    }
}
