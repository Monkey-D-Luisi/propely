// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Permissions.Interfaces;
using Propely.OrgsApi.Application.Permissions.Services;
using Propely.OrgsApi.Domain.Organizations;
using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.UnitTests.Application.Permissions;

public sealed class CachedPermissionEvaluatorTests
{
    private readonly IMembershipRepository _membershipRepository = Substitute.For<IMembershipRepository>();
    private readonly IPermissionOverrideRepository _overrideRepository = Substitute.For<IPermissionOverrideRepository>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly ILogger<CachedPermissionEvaluator> _logger = Substitute.For<ILogger<CachedPermissionEvaluator>>();

    private readonly CachedPermissionEvaluator _sut;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _orgId = Guid.NewGuid();

    public CachedPermissionEvaluatorTests()
    {
        var inner = new PermissionEvaluator(_membershipRepository, _overrideRepository);
        _sut = new CachedPermissionEvaluator(inner, _cacheService, _logger);
    }

    // =========================================================================
    // HasPermissionAsync - Cache miss delegates to inner
    // =========================================================================

    [Fact]
    public async Task HasPermission_CacheMiss_ShouldDelegateToInner()
    {
        // NSubstitute returns default (null) for any GetAsync<T>, simulating cache miss
        SetupMembership(MembershipRole.Owner);
        SetupNoOverrides();

        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        result.Should().BeTrue();
        await _membershipRepository.Received(1).GetAsync(_orgId, _userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HasPermission_CacheMiss_AgentWithNoPermission_ShouldReturnFalse()
    {
        SetupMembership(MembershipRole.Agent);
        SetupNoOverrides();

        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermission_CacheMiss_AgentWithGrantOverride_ShouldReturnTrue()
    {
        SetupMembership(MembershipRole.Agent);
        var ov = PermissionOverride.Create(_userId, _orgId, Permission.PropertiesViewAll, true, Guid.NewGuid());
        _overrideRepository.GetOverridesAsync(_userId, _orgId, Arg.Any<CancellationToken>())
            .Returns(new List<PermissionOverride> { ov });

        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        result.Should().BeTrue();
    }

    // =========================================================================
    // HasPermissionAsync - Cache miss populates cache
    // =========================================================================

    [Fact]
    public async Task HasPermission_CacheMiss_ShouldPopulateCache()
    {
        SetupMembership(MembershipRole.Admin);
        SetupNoOverrides();

        await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        await _cacheService.Received(1).SetAsync(
            $"permissions:{_userId}:{_orgId}",
            Arg.Any<object>(),
            TimeSpan.FromSeconds(30),
            Arg.Any<CancellationToken>());
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Cache miss populates cache
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_CacheMiss_ShouldPopulateCache()
    {
        SetupMembership(MembershipRole.Admin);
        SetupNoOverrides();

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        result.Should().HaveCount(Enum.GetValues<Permission>().Length);
        await _cacheService.Received(1).SetAsync(
            $"permissions:{_userId}:{_orgId}",
            Arg.Any<object>(),
            TimeSpan.FromSeconds(30),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetEffectivePermissions_CacheMiss_ShouldReturnCorrectPermissions()
    {
        SetupMembership(MembershipRole.Agent);
        SetupNoOverrides();

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        result.Should().HaveCount(Enum.GetValues<Permission>().Length);
        var leadsManage = result.Single(p => p.Permission == Permission.LeadsManage);
        leadsManage.Granted.Should().BeTrue();
        leadsManage.Source.Should().Be("Role Default");
    }

    // =========================================================================
    // InvalidateAsync
    // =========================================================================

    [Fact]
    public async Task Invalidate_ShouldRemoveCacheEntry()
    {
        await _sut.InvalidateAsync(_userId, _orgId, CancellationToken.None);

        await _cacheService.Received(1).RemoveAsync(
            $"permissions:{_userId}:{_orgId}",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Invalidate_ShouldUseCorrectCacheKey()
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var orgId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        await _sut.InvalidateAsync(userId, orgId, CancellationToken.None);

        await _cacheService.Received(1).RemoveAsync(
            "permissions:11111111-1111-1111-1111-111111111111:22222222-2222-2222-2222-222222222222",
            Arg.Any<CancellationToken>());
    }

    // =========================================================================
    // CancellationToken propagation
    // =========================================================================

    [Fact]
    public async Task HasPermission_ShouldPropagateCancellationToken()
    {
        SetupMembership(MembershipRole.Owner);
        SetupNoOverrides();
        var ct = new CancellationToken();

        await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, ct);

        await _membershipRepository.Received(1).GetAsync(_orgId, _userId, ct);
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private void SetupMembership(MembershipRole role)
    {
        var membership = Membership.Create(_userId, _orgId, role);
        _membershipRepository.GetAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(membership);
    }

    private void SetupNoOverrides()
    {
        _overrideRepository.GetOverridesAsync(_userId, _orgId, Arg.Any<CancellationToken>())
            .Returns(new List<PermissionOverride>());
    }
}
