// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Permissions.Commands.RemovePermissionOverride;
using Propely.OrgsApi.Application.Permissions.Interfaces;
using Propely.OrgsApi.Application.Permissions.Services;
using Propely.OrgsApi.Domain.Organizations;
using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.UnitTests.Application.Permissions;

public sealed class RemovePermissionOverrideCommandHandlerTests
{
    private readonly IPermissionOverrideRepository _overrideRepository = Substitute.For<IPermissionOverrideRepository>();
    private readonly IMembershipRepository _membershipRepository = Substitute.For<IMembershipRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly ILogger<CachedPermissionEvaluator> _cachedLogger = Substitute.For<ILogger<CachedPermissionEvaluator>>();
    private readonly ILogger<RemovePermissionOverrideCommandHandler> _logger = Substitute.For<ILogger<RemovePermissionOverrideCommandHandler>>();

    private readonly RemovePermissionOverrideCommandHandler _sut;

    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _targetUserId = Guid.NewGuid();
    private readonly Guid _orgId = Guid.NewGuid();

    public RemovePermissionOverrideCommandHandlerTests()
    {
        var inner = new PermissionEvaluator(_membershipRepository, _overrideRepository);
        var cachedEvaluator = new CachedPermissionEvaluator(inner, _cacheService, _cachedLogger);
        _sut = new RemovePermissionOverrideCommandHandler(
            _overrideRepository, _membershipRepository, _unitOfWork, cachedEvaluator, _logger);
    }

    // =========================================================================
    // Authorization checks
    // =========================================================================

    [Fact]
    public async Task Handle_NoMembership_ShouldThrowUnauthorized()
    {
        _membershipRepository.GetAsync(_orgId, _adminId, Arg.Any<CancellationToken>())
            .Returns((Membership?)null);

        var command = new RemovePermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, _adminId);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_AgentRequestor_ShouldThrowUnauthorized()
    {
        SetupMembership(_adminId, MembershipRole.Agent);

        var command = new RemovePermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, _adminId);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // =========================================================================
    // Successful override removal
    // =========================================================================

    [Fact]
    public async Task Handle_ExistingOverride_ShouldRemoveAndSave()
    {
        SetupMembership(_adminId, MembershipRole.Admin);
        SetupMembership(_targetUserId, MembershipRole.Agent);

        var existing = PermissionOverride.Create(_targetUserId, _orgId, Permission.PropertiesViewAll, true, _adminId);
        _overrideRepository.GetOverrideAsync(_targetUserId, _orgId, Permission.PropertiesViewAll, Arg.Any<CancellationToken>())
            .Returns(existing);

        var command = new RemovePermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, _adminId);

        await _sut.Handle(command, CancellationToken.None);

        await _overrideRepository.Received(1).RemoveAsync(existing, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoExistingOverride_ShouldSucceedSilently()
    {
        SetupMembership(_adminId, MembershipRole.Admin);
        SetupMembership(_targetUserId, MembershipRole.Agent);
        _overrideRepository.GetOverrideAsync(_targetUserId, _orgId, Permission.PropertiesViewAll, Arg.Any<CancellationToken>())
            .Returns((PermissionOverride?)null);

        var command = new RemovePermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, _adminId);

        await _sut.Handle(command, CancellationToken.None);

        await _overrideRepository.DidNotReceive().RemoveAsync(Arg.Any<PermissionOverride>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // =========================================================================
    // Target validation
    // =========================================================================

    [Fact]
    public async Task Handle_TargetIsOwner_ShouldThrowInvalidOperation()
    {
        SetupMembership(_adminId, MembershipRole.Admin);
        SetupMembership(_targetUserId, MembershipRole.Owner);

        var command = new RemovePermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, _adminId);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*owners*");
    }

    [Fact]
    public async Task Handle_TargetNotMember_ShouldThrowInvalidOperation()
    {
        SetupMembership(_adminId, MembershipRole.Admin);
        _membershipRepository.GetAsync(_orgId, _targetUserId, Arg.Any<CancellationToken>())
            .Returns((Membership?)null);

        var command = new RemovePermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, _adminId);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not a member*");
    }

    // =========================================================================
    // Cache invalidation
    // =========================================================================

    [Fact]
    public async Task Handle_ShouldInvalidateCache()
    {
        SetupMembership(_adminId, MembershipRole.Admin);
        SetupMembership(_targetUserId, MembershipRole.Agent);

        var existing = PermissionOverride.Create(_targetUserId, _orgId, Permission.PropertiesViewAll, true, _adminId);
        _overrideRepository.GetOverrideAsync(_targetUserId, _orgId, Permission.PropertiesViewAll, Arg.Any<CancellationToken>())
            .Returns(existing);

        var command = new RemovePermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, _adminId);

        await _sut.Handle(command, CancellationToken.None);

        await _cacheService.Received(1).RemoveAsync(
            $"permissions:{_targetUserId}:{_orgId}",
            Arg.Any<CancellationToken>());
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private void SetupMembership(Guid userId, MembershipRole role)
    {
        var membership = Membership.Create(userId, _orgId, role);
        _membershipRepository.GetAsync(_orgId, userId, Arg.Any<CancellationToken>())
            .Returns(membership);
    }
}
