// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Permissions.Commands.SetPermissionOverride;
using Propely.OrgsApi.Application.Permissions.Interfaces;
using Propely.OrgsApi.Application.Permissions.Services;
using Propely.OrgsApi.Domain.Organizations;
using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.UnitTests.Application.Permissions;

public sealed class SetPermissionOverrideCommandHandlerTests
{
    private readonly IPermissionOverrideRepository _overrideRepository = Substitute.For<IPermissionOverrideRepository>();
    private readonly IMembershipRepository _membershipRepository = Substitute.For<IMembershipRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly ILogger<CachedPermissionEvaluator> _cachedLogger = Substitute.For<ILogger<CachedPermissionEvaluator>>();
    private readonly ILogger<SetPermissionOverrideCommandHandler> _logger = Substitute.For<ILogger<SetPermissionOverrideCommandHandler>>();

    private readonly SetPermissionOverrideCommandHandler _sut;
    private readonly CachedPermissionEvaluator _cachedEvaluator;

    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _targetUserId = Guid.NewGuid();
    private readonly Guid _orgId = Guid.NewGuid();

    public SetPermissionOverrideCommandHandlerTests()
    {
        var inner = new PermissionEvaluator(_membershipRepository, _overrideRepository);
        _cachedEvaluator = new CachedPermissionEvaluator(inner, _cacheService, _cachedLogger);
        _sut = new SetPermissionOverrideCommandHandler(
            _overrideRepository, _membershipRepository, _unitOfWork, _cachedEvaluator, _logger);
    }

    // =========================================================================
    // Authorization checks
    // =========================================================================

    [Fact]
    public async Task Handle_NoMembership_ShouldThrowUnauthorized()
    {
        _membershipRepository.GetAsync(_orgId, _adminId, Arg.Any<CancellationToken>())
            .Returns((Membership?)null);

        var command = new SetPermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, true, _adminId);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_AgentRequestor_ShouldThrowUnauthorized()
    {
        SetupMembership(_adminId, MembershipRole.Agent);

        var command = new SetPermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, true, _adminId);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*admins and owners*");
    }

    [Fact]
    public async Task Handle_ViewerRequestor_ShouldThrowUnauthorized()
    {
        SetupMembership(_adminId, MembershipRole.Viewer);

        var command = new SetPermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, true, _adminId);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_TargetIsOwner_ShouldThrowInvalidOperation()
    {
        SetupMembership(_adminId, MembershipRole.Admin);
        SetupMembership(_targetUserId, MembershipRole.Owner);

        var command = new SetPermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, true, _adminId);

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

        var command = new SetPermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, true, _adminId);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not a member*");
    }

    // =========================================================================
    // Successful override creation
    // =========================================================================

    [Fact]
    public async Task Handle_AdminCreatesOverride_ShouldSucceed()
    {
        SetupMembership(_adminId, MembershipRole.Admin);
        SetupMembership(_targetUserId, MembershipRole.Agent);
        _overrideRepository.GetOverrideAsync(_targetUserId, _orgId, Permission.PropertiesViewAll, Arg.Any<CancellationToken>())
            .Returns((PermissionOverride?)null);

        var command = new SetPermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, true, _adminId);

        await _sut.Handle(command, CancellationToken.None);

        await _overrideRepository.Received(1).AddAsync(Arg.Any<PermissionOverride>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OwnerCreatesOverride_ShouldSucceed()
    {
        SetupMembership(_adminId, MembershipRole.Owner);
        SetupMembership(_targetUserId, MembershipRole.Agent);
        _overrideRepository.GetOverrideAsync(_targetUserId, _orgId, Permission.PropertiesViewAll, Arg.Any<CancellationToken>())
            .Returns((PermissionOverride?)null);

        var command = new SetPermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, true, _adminId);

        await _sut.Handle(command, CancellationToken.None);

        await _overrideRepository.Received(1).AddAsync(Arg.Any<PermissionOverride>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // =========================================================================
    // Replacing existing override
    // =========================================================================

    [Fact]
    public async Task Handle_ExistingOverride_ShouldRemoveOldAndCreateNew()
    {
        SetupMembership(_adminId, MembershipRole.Admin);
        SetupMembership(_targetUserId, MembershipRole.Agent);

        var existing = PermissionOverride.Create(_targetUserId, _orgId, Permission.PropertiesViewAll, false, _adminId);
        _overrideRepository.GetOverrideAsync(_targetUserId, _orgId, Permission.PropertiesViewAll, Arg.Any<CancellationToken>())
            .Returns(existing);

        var command = new SetPermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, true, _adminId);

        await _sut.Handle(command, CancellationToken.None);

        await _overrideRepository.Received(1).RemoveAsync(existing, Arg.Any<CancellationToken>());
        await _overrideRepository.Received(1).AddAsync(Arg.Any<PermissionOverride>(), Arg.Any<CancellationToken>());
    }

    // =========================================================================
    // Cache invalidation
    // =========================================================================

    [Fact]
    public async Task Handle_ShouldInvalidateCache()
    {
        SetupMembership(_adminId, MembershipRole.Admin);
        SetupMembership(_targetUserId, MembershipRole.Agent);
        _overrideRepository.GetOverrideAsync(_targetUserId, _orgId, Permission.PropertiesViewAll, Arg.Any<CancellationToken>())
            .Returns((PermissionOverride?)null);

        var command = new SetPermissionOverrideCommand(_orgId, _targetUserId, Permission.PropertiesViewAll, true, _adminId);

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
