// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Permissions.Interfaces;
using Propely.OrgsApi.Application.Permissions.Services;
using Propely.OrgsApi.Domain.Organizations;
using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.UnitTests.Application.Permissions;

public sealed class PermissionEvaluatorTests
{
    private readonly IMembershipRepository _membershipRepository = Substitute.For<IMembershipRepository>();
    private readonly IPermissionOverrideRepository _overrideRepository = Substitute.For<IPermissionOverrideRepository>();
    private readonly PermissionEvaluator _sut;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _orgId = Guid.NewGuid();

    public PermissionEvaluatorTests()
    {
        _sut = new PermissionEvaluator(_membershipRepository, _overrideRepository);
    }

    private Membership CreateMembership(MembershipRole role)
    {
        return Membership.Create(_userId, _orgId, role);
    }

    private void SetupMembership(MembershipRole role)
    {
        _membershipRepository.GetAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(CreateMembership(role));
    }

    private void SetupNoMembership()
    {
        _membershipRepository.GetAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns((Membership?)null);
    }

    private void SetupOverride(Permission permission, bool granted)
    {
        var ov = PermissionOverride.Create(_userId, _orgId, permission, granted, Guid.NewGuid());
        _overrideRepository.GetOverrideAsync(_userId, _orgId, permission, Arg.Any<CancellationToken>())
            .Returns(ov);
    }

    private void SetupOverrides(params (Permission permission, bool granted)[] overrides)
    {
        var list = overrides.Select(o =>
            PermissionOverride.Create(_userId, _orgId, o.permission, o.granted, Guid.NewGuid()))
            .ToList();
        _overrideRepository.GetOverridesAsync(_userId, _orgId, Arg.Any<CancellationToken>())
            .Returns(list);
    }

    private void SetupNoOverride(Permission permission)
    {
        _overrideRepository.GetOverrideAsync(_userId, _orgId, permission, Arg.Any<CancellationToken>())
            .Returns((PermissionOverride?)null);
    }

    private void SetupNoOverrides()
    {
        _overrideRepository.GetOverridesAsync(_userId, _orgId, Arg.Any<CancellationToken>())
            .Returns(new List<PermissionOverride>());
    }

    // =========================================================================
    // HasPermissionAsync - No membership
    // =========================================================================

    [Fact]
    public async Task HasPermission_NoMembership_ShouldReturnFalse()
    {
        SetupNoMembership();

        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        result.Should().BeFalse();
    }

    // =========================================================================
    // HasPermissionAsync - Owner (always all permissions)
    // =========================================================================

    [Theory]
    [InlineData(Permission.PropertiesViewAll)]
    [InlineData(Permission.PropertiesEditAll)]
    [InlineData(Permission.ContactsViewAll)]
    [InlineData(Permission.ContactsEditAll)]
    [InlineData(Permission.AppointmentsViewAll)]
    [InlineData(Permission.PublishingManage)]
    [InlineData(Permission.LeadsManage)]
    [InlineData(Permission.ReportsView)]
    public async Task HasPermission_Owner_ShouldAlwaysReturnTrue(Permission permission)
    {
        SetupMembership(MembershipRole.Owner);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, permission, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(Permission.PropertiesViewAll)]
    [InlineData(Permission.PropertiesEditAll)]
    [InlineData(Permission.ContactsViewAll)]
    [InlineData(Permission.LeadsManage)]
    public async Task HasPermission_Owner_WithDenyOverride_ShouldStillReturnTrue(Permission permission)
    {
        SetupMembership(MembershipRole.Owner);
        // Owner permissions cannot be restricted by overrides

        var result = await _sut.HasPermissionAsync(_userId, _orgId, permission, CancellationToken.None);

        result.Should().BeTrue();
    }

    // =========================================================================
    // HasPermissionAsync - Admin (all permissions by default)
    // =========================================================================

    [Theory]
    [InlineData(Permission.PropertiesViewAll)]
    [InlineData(Permission.PropertiesEditAll)]
    [InlineData(Permission.ContactsViewAll)]
    [InlineData(Permission.ContactsEditAll)]
    [InlineData(Permission.AppointmentsViewAll)]
    [InlineData(Permission.PublishingManage)]
    [InlineData(Permission.LeadsManage)]
    [InlineData(Permission.ReportsView)]
    public async Task HasPermission_Admin_WithNoOverride_ShouldReturnTrue(Permission permission)
    {
        SetupMembership(MembershipRole.Admin);
        SetupNoOverride(permission);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, permission, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermission_Admin_WithDenyOverride_ShouldReturnFalse()
    {
        SetupMembership(MembershipRole.Admin);
        SetupOverride(Permission.PropertiesViewAll, false);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        result.Should().BeFalse();
    }

    // =========================================================================
    // HasPermissionAsync - Agent (only LeadsManage by default)
    // =========================================================================

    [Fact]
    public async Task HasPermission_Agent_LeadsManage_WithNoOverride_ShouldReturnTrue()
    {
        SetupMembership(MembershipRole.Agent);
        SetupNoOverride(Permission.LeadsManage);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission.LeadsManage, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(Permission.PropertiesViewAll)]
    [InlineData(Permission.PropertiesEditAll)]
    [InlineData(Permission.ContactsViewAll)]
    [InlineData(Permission.ContactsEditAll)]
    [InlineData(Permission.AppointmentsViewAll)]
    [InlineData(Permission.PublishingManage)]
    [InlineData(Permission.ReportsView)]
    public async Task HasPermission_Agent_NonDefaultPermission_WithNoOverride_ShouldReturnFalse(Permission permission)
    {
        SetupMembership(MembershipRole.Agent);
        SetupNoOverride(permission);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, permission, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermission_Agent_WithGrantOverride_ShouldReturnTrue()
    {
        SetupMembership(MembershipRole.Agent);
        SetupOverride(Permission.PropertiesViewAll, true);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermission_Agent_LeadsManage_WithDenyOverride_ShouldReturnFalse()
    {
        SetupMembership(MembershipRole.Agent);
        SetupOverride(Permission.LeadsManage, false);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission.LeadsManage, CancellationToken.None);

        result.Should().BeFalse();
    }

    // =========================================================================
    // HasPermissionAsync - Viewer (no permissions by default)
    // =========================================================================

    [Theory]
    [InlineData(Permission.PropertiesViewAll)]
    [InlineData(Permission.PropertiesEditAll)]
    [InlineData(Permission.ContactsViewAll)]
    [InlineData(Permission.ContactsEditAll)]
    [InlineData(Permission.AppointmentsViewAll)]
    [InlineData(Permission.PublishingManage)]
    [InlineData(Permission.LeadsManage)]
    [InlineData(Permission.ReportsView)]
    public async Task HasPermission_Viewer_WithNoOverride_ShouldReturnFalse(Permission permission)
    {
        SetupMembership(MembershipRole.Viewer);
        SetupNoOverride(permission);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, permission, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermission_Viewer_WithGrantOverride_ShouldReturnTrue()
    {
        SetupMembership(MembershipRole.Viewer);
        SetupOverride(Permission.ReportsView, true);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission.ReportsView, CancellationToken.None);

        result.Should().BeTrue();
    }

    // =========================================================================
    // HasPermissionAsync - Deleted membership
    // =========================================================================

    [Fact]
    public async Task HasPermission_DeletedMembership_ShouldReturnFalse()
    {
        var membership = CreateMembership(MembershipRole.Admin);
        membership.SoftDelete();
        _membershipRepository.GetAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(membership);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        result.Should().BeFalse();
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Owner
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_Owner_ShouldReturnAllGrantedAsRoleDefault()
    {
        SetupMembership(MembershipRole.Owner);
        SetupNoOverrides();

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        result.Should().HaveCount(Enum.GetValues<Permission>().Length);
        result.Should().AllSatisfy(p =>
        {
            p.Granted.Should().BeTrue();
            p.Source.Should().Be("Role Default");
        });
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Admin
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_Admin_WithNoOverrides_ShouldReturnAllGrantedAsRoleDefault()
    {
        SetupMembership(MembershipRole.Admin);
        SetupNoOverrides();

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        result.Should().HaveCount(Enum.GetValues<Permission>().Length);
        result.Should().AllSatisfy(p =>
        {
            p.Granted.Should().BeTrue();
            p.Source.Should().Be("Role Default");
        });
    }

    [Fact]
    public async Task GetEffectivePermissions_Admin_WithDenyOverride_ShouldShowOverrideSource()
    {
        SetupMembership(MembershipRole.Admin);
        SetupOverrides((Permission.PropertiesViewAll, false));

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        var propertiesViewAll = result.Single(p => p.Permission == Permission.PropertiesViewAll);
        propertiesViewAll.Granted.Should().BeFalse();
        propertiesViewAll.Source.Should().Be("Override");
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Agent
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_Agent_WithNoOverrides_ShouldReturnOnlyLeadsManageGranted()
    {
        SetupMembership(MembershipRole.Agent);
        SetupNoOverrides();

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        result.Should().HaveCount(Enum.GetValues<Permission>().Length);

        var leadsManage = result.Single(p => p.Permission == Permission.LeadsManage);
        leadsManage.Granted.Should().BeTrue();
        leadsManage.Source.Should().Be("Role Default");

        var others = result.Where(p => p.Permission != Permission.LeadsManage);
        others.Should().AllSatisfy(p =>
        {
            p.Granted.Should().BeFalse();
            p.Source.Should().Be("Role Default");
        });
    }

    [Fact]
    public async Task GetEffectivePermissions_Agent_WithGrantOverride_ShouldShowOverrideSource()
    {
        SetupMembership(MembershipRole.Agent);
        SetupOverrides((Permission.PropertiesViewAll, true));

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        var propertiesViewAll = result.Single(p => p.Permission == Permission.PropertiesViewAll);
        propertiesViewAll.Granted.Should().BeTrue();
        propertiesViewAll.Source.Should().Be("Override");
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Viewer
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_Viewer_WithNoOverrides_ShouldReturnAllDeniedAsRoleDefault()
    {
        SetupMembership(MembershipRole.Viewer);
        SetupNoOverrides();

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        result.Should().HaveCount(Enum.GetValues<Permission>().Length);
        result.Should().AllSatisfy(p =>
        {
            p.Granted.Should().BeFalse();
            p.Source.Should().Be("Role Default");
        });
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - No membership
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_NoMembership_ShouldReturnEmptyList()
    {
        SetupNoMembership();

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        result.Should().BeEmpty();
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Deleted membership
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_DeletedMembership_ShouldReturnEmptyList()
    {
        var membership = CreateMembership(MembershipRole.Admin);
        membership.SoftDelete();
        _membershipRepository.GetAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(membership);

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        result.Should().BeEmpty();
    }

    // =========================================================================
    // CancellationToken propagation
    // =========================================================================

    [Fact]
    public async Task HasPermission_ShouldPropagateCancellationToken()
    {
        SetupMembership(MembershipRole.Agent);
        SetupNoOverride(Permission.LeadsManage);
        var ct = new CancellationToken();

        await _sut.HasPermissionAsync(_userId, _orgId, Permission.LeadsManage, ct);

        await _membershipRepository.Received(1).GetAsync(_orgId, _userId, ct);
    }

    [Fact]
    public async Task GetEffectivePermissions_ShouldPropagateCancellationToken()
    {
        SetupMembership(MembershipRole.Agent);
        SetupNoOverrides();
        var ct = new CancellationToken();

        await _sut.GetEffectivePermissionsAsync(_userId, _orgId, ct);

        await _membershipRepository.Received(1).GetAsync(_orgId, _userId, ct);
        await _overrideRepository.Received(1).GetOverridesAsync(_userId, _orgId, ct);
    }
}
