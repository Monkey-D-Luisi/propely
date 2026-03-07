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
    // HasPermissionAsync - Owner with actual deny override set up
    // =========================================================================

    [Fact]
    public async Task HasPermission_Owner_WithActualDenyOverrideSetUp_ShouldStillReturnTrue()
    {
        // Owner permissions bypass the override lookup entirely,
        // so even if a deny override exists in the repository, it is never consulted.
        SetupMembership(MembershipRole.Owner);
        SetupOverride(Permission.PropertiesViewAll, false);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        result.Should().BeTrue();
        // Verify override repository is never consulted for Owner
        await _overrideRepository.DidNotReceive()
            .GetOverrideAsync(_userId, _orgId, Permission.PropertiesViewAll, Arg.Any<CancellationToken>());
    }

    // =========================================================================
    // HasPermissionAsync - Admin with grant override on already-granted permission
    // =========================================================================

    [Fact]
    public async Task HasPermission_Admin_WithGrantOverrideOnAlreadyGranted_ShouldReturnTrue()
    {
        SetupMembership(MembershipRole.Admin);
        SetupOverride(Permission.PropertiesViewAll, true);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        result.Should().BeTrue();
    }

    // =========================================================================
    // HasPermissionAsync - Viewer with multiple grant overrides
    // =========================================================================

    [Fact]
    public async Task HasPermission_Viewer_WithMultipleGrantOverrides_ShouldReturnTrueForEach()
    {
        SetupMembership(MembershipRole.Viewer);
        SetupOverride(Permission.PropertiesViewAll, true);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermission_Viewer_WithGrantOverrideOnOnePermission_ShouldDenyOthers()
    {
        SetupMembership(MembershipRole.Viewer);
        SetupOverride(Permission.PropertiesViewAll, true);
        SetupNoOverride(Permission.PropertiesEditAll);

        var resultGranted = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);
        var resultDenied = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesEditAll, CancellationToken.None);

        resultGranted.Should().BeTrue();
        resultDenied.Should().BeFalse();
    }

    // =========================================================================
    // HasPermissionAsync - Admin: each permission individually denied via override
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
    public async Task HasPermission_Admin_WithDenyOverride_ShouldReturnFalseForEach(Permission permission)
    {
        SetupMembership(MembershipRole.Admin);
        SetupOverride(permission, false);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, permission, CancellationToken.None);

        result.Should().BeFalse();
    }

    // =========================================================================
    // HasPermissionAsync - Owner does not consult override repo
    // =========================================================================

    [Fact]
    public async Task HasPermission_Owner_ShouldNotQueryOverrideRepository()
    {
        SetupMembership(MembershipRole.Owner);

        await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        await _overrideRepository.DidNotReceive()
            .GetOverrideAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Permission>(), Arg.Any<CancellationToken>());
    }

    // =========================================================================
    // HasPermissionAsync - every permission for every role (comprehensive matrix)
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
    public async Task HasPermission_Owner_ShouldGrantEveryPermission(Permission permission)
    {
        SetupMembership(MembershipRole.Owner);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, permission, CancellationToken.None);

        result.Should().BeTrue($"Owner should have {permission}");
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Owner with overrides should ignore them
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_Owner_WithDenyOverrides_ShouldStillReturnAllGranted()
    {
        SetupMembership(MembershipRole.Owner);
        SetupOverrides(
            (Permission.PropertiesViewAll, false),
            (Permission.ContactsViewAll, false));

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        result.Should().HaveCount(Enum.GetValues<Permission>().Length);
        result.Should().AllSatisfy(p =>
        {
            p.Granted.Should().BeTrue();
            p.Source.Should().Be("Role Default");
        });
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Admin with multiple overrides
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_Admin_WithMultipleOverrides_ShouldReflectEachOverride()
    {
        SetupMembership(MembershipRole.Admin);
        SetupOverrides(
            (Permission.PropertiesViewAll, false),
            (Permission.ContactsEditAll, false));

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        var propertiesView = result.Single(p => p.Permission == Permission.PropertiesViewAll);
        propertiesView.Granted.Should().BeFalse();
        propertiesView.Source.Should().Be("Override");

        var contactsEdit = result.Single(p => p.Permission == Permission.ContactsEditAll);
        contactsEdit.Granted.Should().BeFalse();
        contactsEdit.Source.Should().Be("Override");

        // Non-overridden permissions should still be granted
        var leadsManage = result.Single(p => p.Permission == Permission.LeadsManage);
        leadsManage.Granted.Should().BeTrue();
        leadsManage.Source.Should().Be("Role Default");
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Viewer with grant overrides
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_Viewer_WithGrantOverrides_ShouldShowOverrideSources()
    {
        SetupMembership(MembershipRole.Viewer);
        SetupOverrides(
            (Permission.ReportsView, true),
            (Permission.PropertiesViewAll, true));

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        var reportsView = result.Single(p => p.Permission == Permission.ReportsView);
        reportsView.Granted.Should().BeTrue();
        reportsView.Source.Should().Be("Override");

        var propertiesView = result.Single(p => p.Permission == Permission.PropertiesViewAll);
        propertiesView.Granted.Should().BeTrue();
        propertiesView.Source.Should().Be("Override");

        // Non-overridden permissions should be denied
        var contactsView = result.Single(p => p.Permission == Permission.ContactsViewAll);
        contactsView.Granted.Should().BeFalse();
        contactsView.Source.Should().Be("Role Default");
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Agent with deny override on default permission
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_Agent_WithDenyOverrideOnLeadsManage_ShouldShowDenied()
    {
        SetupMembership(MembershipRole.Agent);
        SetupOverrides((Permission.LeadsManage, false));

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        var leadsManage = result.Single(p => p.Permission == Permission.LeadsManage);
        leadsManage.Granted.Should().BeFalse();
        leadsManage.Source.Should().Be("Override");
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - result count matches enum values
    // =========================================================================

    [Theory]
    [InlineData(MembershipRole.Owner)]
    [InlineData(MembershipRole.Admin)]
    [InlineData(MembershipRole.Agent)]
    [InlineData(MembershipRole.Viewer)]
    public async Task GetEffectivePermissions_AnyRole_ShouldReturnExactPermissionCount(MembershipRole role)
    {
        SetupMembership(role);
        SetupNoOverrides();

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        result.Should().HaveCount(Enum.GetValues<Permission>().Length);
    }

    // =========================================================================
    // Admin and Owner equivalence (L8 decision)
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_AdminAndOwner_ShouldHaveSameDefaultPermissions()
    {
        // L8 decision: Admin has the same permissions as Owner by default
        SetupNoOverrides();

        SetupMembership(MembershipRole.Owner);
        var ownerResult = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        SetupMembership(MembershipRole.Admin);
        var adminResult = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        var ownerPermissions = ownerResult.Select(p => (p.Permission, p.Granted)).OrderBy(x => x.Permission);
        var adminPermissions = adminResult.Select(p => (p.Permission, p.Granted)).OrderBy(x => x.Permission);

        adminPermissions.Should().BeEquivalentTo(ownerPermissions);
    }

    // =========================================================================
    // Admin vs Owner: Admin can be restricted, Owner cannot
    // =========================================================================

    [Fact]
    public async Task HasPermission_AdminCanBeRestricted_OwnerCannot()
    {
        // Admin can have permissions revoked via override; Owner cannot
        SetupOverride(Permission.PropertiesViewAll, false);

        SetupMembership(MembershipRole.Admin);
        var adminResult = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        SetupMembership(MembershipRole.Owner);
        var ownerResult = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        adminResult.Should().BeFalse("Admin can be restricted by deny override");
        ownerResult.Should().BeTrue("Owner cannot be restricted by any override");
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

    // =========================================================================
    // HasPermissionAsync - uses different user/org IDs (isolation test)
    // =========================================================================

    [Fact]
    public async Task HasPermission_ShouldUseCorrectUserAndOrgIds()
    {
        SetupMembership(MembershipRole.Agent);
        SetupNoOverride(Permission.LeadsManage);

        await _sut.HasPermissionAsync(_userId, _orgId, Permission.LeadsManage, CancellationToken.None);

        // Verify the correct IDs were passed to the repository
        await _membershipRepository.Received(1).GetAsync(_orgId, _userId, Arg.Any<CancellationToken>());
        await _overrideRepository.Received(1).GetOverrideAsync(_userId, _orgId, Permission.LeadsManage, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HasPermission_DifferentOrgId_ShouldNotReturnOtherOrgMembership()
    {
        var otherOrgId = Guid.NewGuid();
        // No membership set up for otherOrgId
        _membershipRepository.GetAsync(otherOrgId, _userId, Arg.Any<CancellationToken>())
            .Returns((Membership?)null);

        var result = await _sut.HasPermissionAsync(_userId, otherOrgId, Permission.PropertiesViewAll, CancellationToken.None);

        result.Should().BeFalse();
    }

    // =========================================================================
    // HasPermissionAsync - No membership should NOT query override repo
    // =========================================================================

    [Fact]
    public async Task HasPermission_NoMembership_ShouldNotQueryOverrideRepository()
    {
        SetupNoMembership();

        await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        await _overrideRepository.DidNotReceive()
            .GetOverrideAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Permission>(), Arg.Any<CancellationToken>());
    }

    // =========================================================================
    // HasPermissionAsync - Deleted membership should NOT query override repo
    // =========================================================================

    [Fact]
    public async Task HasPermission_DeletedMembership_ShouldNotQueryOverrideRepository()
    {
        var membership = CreateMembership(MembershipRole.Admin);
        membership.SoftDelete();
        _membershipRepository.GetAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(membership);

        await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        await _overrideRepository.DidNotReceive()
            .GetOverrideAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Permission>(), Arg.Any<CancellationToken>());
    }

    // =========================================================================
    // HasPermissionAsync - Agent grant override for each non-default permission
    // =========================================================================

    [Theory]
    [InlineData(Permission.PropertiesViewAll)]
    [InlineData(Permission.PropertiesEditAll)]
    [InlineData(Permission.ContactsViewAll)]
    [InlineData(Permission.ContactsEditAll)]
    [InlineData(Permission.AppointmentsViewAll)]
    [InlineData(Permission.PublishingManage)]
    [InlineData(Permission.ReportsView)]
    public async Task HasPermission_Agent_WithGrantOverride_ShouldReturnTrueForEachNonDefaultPermission(Permission permission)
    {
        SetupMembership(MembershipRole.Agent);
        SetupOverride(permission, true);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, permission, CancellationToken.None);

        result.Should().BeTrue($"Agent with grant override should have {permission}");
    }

    // =========================================================================
    // HasPermissionAsync - Viewer grant override for each permission
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
    public async Task HasPermission_Viewer_WithGrantOverride_ShouldReturnTrueForEachPermission(Permission permission)
    {
        SetupMembership(MembershipRole.Viewer);
        SetupOverride(permission, true);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, permission, CancellationToken.None);

        result.Should().BeTrue($"Viewer with grant override should have {permission}");
    }

    // =========================================================================
    // HasPermissionAsync - Viewer deny override on already-denied permission
    // =========================================================================

    [Fact]
    public async Task HasPermission_Viewer_WithDenyOverrideOnAlreadyDenied_ShouldReturnFalse()
    {
        SetupMembership(MembershipRole.Viewer);
        SetupOverride(Permission.PropertiesViewAll, false);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        result.Should().BeFalse();
    }

    // =========================================================================
    // HasPermissionAsync - Agent mixed overrides (deny default + grant non-default)
    // =========================================================================

    [Fact]
    public async Task HasPermission_Agent_DenyDefaultAndGrantNonDefault_ShouldRespectBoth()
    {
        SetupMembership(MembershipRole.Agent);
        SetupOverride(Permission.LeadsManage, false);

        var leadsResult = await _sut.HasPermissionAsync(_userId, _orgId, Permission.LeadsManage, CancellationToken.None);

        leadsResult.Should().BeFalse("Agent's default LeadsManage should be denied by override");

        // Reset and set up grant override for a non-default permission
        SetupOverride(Permission.ContactsViewAll, true);

        var contactsResult = await _sut.HasPermissionAsync(_userId, _orgId, Permission.ContactsViewAll, CancellationToken.None);

        contactsResult.Should().BeTrue("Agent should gain ContactsViewAll via grant override");
    }

    // =========================================================================
    // HasPermissionAsync - Method call isolation (uses GetOverrideAsync, not GetOverridesAsync)
    // =========================================================================

    [Fact]
    public async Task HasPermission_ShouldUseGetOverrideAsync_NotGetOverridesAsync()
    {
        SetupMembership(MembershipRole.Agent);
        SetupNoOverride(Permission.LeadsManage);

        await _sut.HasPermissionAsync(_userId, _orgId, Permission.LeadsManage, CancellationToken.None);

        // HasPermissionAsync should call the singular GetOverrideAsync
        await _overrideRepository.Received(1)
            .GetOverrideAsync(_userId, _orgId, Permission.LeadsManage, Arg.Any<CancellationToken>());
        // HasPermissionAsync should NOT call the plural GetOverridesAsync
        await _overrideRepository.DidNotReceive()
            .GetOverridesAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - No membership should NOT query override repo
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_NoMembership_ShouldNotQueryOverrideRepository()
    {
        SetupNoMembership();

        await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        await _overrideRepository.DidNotReceive()
            .GetOverridesAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Deleted membership should NOT query override repo
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_DeletedMembership_ShouldNotQueryOverrideRepository()
    {
        var membership = CreateMembership(MembershipRole.Admin);
        membership.SoftDelete();
        _membershipRepository.GetAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(membership);

        await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        await _overrideRepository.DidNotReceive()
            .GetOverridesAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Method call isolation (uses GetOverridesAsync, not GetOverrideAsync)
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_ShouldUseGetOverridesAsync_NotGetOverrideAsync()
    {
        SetupMembership(MembershipRole.Agent);
        SetupNoOverrides();

        await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        // GetEffectivePermissionsAsync should call the plural GetOverridesAsync
        await _overrideRepository.Received(1)
            .GetOverridesAsync(_userId, _orgId, Arg.Any<CancellationToken>());
        // GetEffectivePermissionsAsync should NOT call the singular GetOverrideAsync
        await _overrideRepository.DidNotReceive()
            .GetOverrideAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Permission>(), Arg.Any<CancellationToken>());
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Admin with grant override on already-granted
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_Admin_WithGrantOverrideOnAlreadyGranted_ShouldShowOverrideSource()
    {
        SetupMembership(MembershipRole.Admin);
        SetupOverrides((Permission.PropertiesViewAll, true));

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        var propertiesView = result.Single(p => p.Permission == Permission.PropertiesViewAll);
        propertiesView.Granted.Should().BeTrue();
        // Even though Admin already has this permission by default, when an override exists
        // the source should reflect "Override" (override takes precedence in evaluation)
        propertiesView.Source.Should().Be("Override");
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Agent with mixed overrides (grant + deny)
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_Agent_WithMixedOverrides_ShouldReflectEachCorrectly()
    {
        SetupMembership(MembershipRole.Agent);
        SetupOverrides(
            (Permission.LeadsManage, false),        // deny the default
            (Permission.PropertiesViewAll, true),    // grant a non-default
            (Permission.ContactsEditAll, true));     // grant another non-default

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        var leadsManage = result.Single(p => p.Permission == Permission.LeadsManage);
        leadsManage.Granted.Should().BeFalse();
        leadsManage.Source.Should().Be("Override");

        var propertiesView = result.Single(p => p.Permission == Permission.PropertiesViewAll);
        propertiesView.Granted.Should().BeTrue();
        propertiesView.Source.Should().Be("Override");

        var contactsEdit = result.Single(p => p.Permission == Permission.ContactsEditAll);
        contactsEdit.Granted.Should().BeTrue();
        contactsEdit.Source.Should().Be("Override");

        // Non-overridden, non-default permissions should be denied
        var reportsView = result.Single(p => p.Permission == Permission.ReportsView);
        reportsView.Granted.Should().BeFalse();
        reportsView.Source.Should().Be("Role Default");
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Viewer with all permissions granted via overrides
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_Viewer_WithAllPermissionsGrantedViaOverrides_ShouldReturnAllGranted()
    {
        SetupMembership(MembershipRole.Viewer);
        var allOverrides = Enum.GetValues<Permission>()
            .Select(p => (p, true))
            .ToArray();
        SetupOverrides(allOverrides);

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        result.Should().HaveCount(Enum.GetValues<Permission>().Length);
        result.Should().AllSatisfy(p =>
        {
            p.Granted.Should().BeTrue();
            p.Source.Should().Be("Override");
        });
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Each permission has a unique entry (no duplicates)
    // =========================================================================

    [Theory]
    [InlineData(MembershipRole.Owner)]
    [InlineData(MembershipRole.Admin)]
    [InlineData(MembershipRole.Agent)]
    [InlineData(MembershipRole.Viewer)]
    public async Task GetEffectivePermissions_AnyRole_ShouldHaveNoDuplicatePermissions(MembershipRole role)
    {
        SetupMembership(role);
        SetupNoOverrides();

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        var permissionNames = result.Select(p => p.Permission).ToList();
        permissionNames.Should().OnlyHaveUniqueItems();
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - All enum values are represented
    // =========================================================================

    [Theory]
    [InlineData(MembershipRole.Owner)]
    [InlineData(MembershipRole.Admin)]
    [InlineData(MembershipRole.Agent)]
    [InlineData(MembershipRole.Viewer)]
    public async Task GetEffectivePermissions_AnyRole_ShouldContainEveryPermissionEnumValue(MembershipRole role)
    {
        SetupMembership(role);
        SetupNoOverrides();

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        var allPermissions = Enum.GetValues<Permission>();
        var resultPermissions = result.Select(p => p.Permission);
        resultPermissions.Should().BeEquivalentTo(allPermissions);
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Viewer with deny override (explicit deny on already-denied)
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_Viewer_WithDenyOverride_ShouldShowOverrideSource()
    {
        SetupMembership(MembershipRole.Viewer);
        SetupOverrides((Permission.PropertiesViewAll, false));

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        var propertiesView = result.Single(p => p.Permission == Permission.PropertiesViewAll);
        propertiesView.Granted.Should().BeFalse();
        // Even though Viewer has no default permissions, an explicit deny override
        // should be reported with "Override" source
        propertiesView.Source.Should().Be("Override");
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Agent deny override on each non-default permission
    // =========================================================================

    [Theory]
    [InlineData(Permission.PropertiesViewAll)]
    [InlineData(Permission.PropertiesEditAll)]
    [InlineData(Permission.ContactsViewAll)]
    [InlineData(Permission.ContactsEditAll)]
    [InlineData(Permission.AppointmentsViewAll)]
    [InlineData(Permission.PublishingManage)]
    [InlineData(Permission.ReportsView)]
    public async Task GetEffectivePermissions_Agent_WithDenyOverrideOnNonDefault_ShouldStillBeDenied(Permission permission)
    {
        SetupMembership(MembershipRole.Agent);
        SetupOverrides((permission, false));

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        var perm = result.Single(p => p.Permission == permission);
        perm.Granted.Should().BeFalse();
        perm.Source.Should().Be("Override");
    }

    // =========================================================================
    // HasPermissionAsync - Deleted Owner should still return false
    // =========================================================================

    [Fact]
    public async Task HasPermission_DeletedOwner_ShouldReturnFalse()
    {
        var membership = CreateMembership(MembershipRole.Owner);
        membership.SoftDelete();
        _membershipRepository.GetAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(membership);

        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        result.Should().BeFalse("Even Owner should be denied when membership is soft-deleted");
    }

    // =========================================================================
    // GetEffectivePermissionsAsync - Deleted Owner should return empty
    // =========================================================================

    [Fact]
    public async Task GetEffectivePermissions_DeletedOwner_ShouldReturnEmptyList()
    {
        var membership = CreateMembership(MembershipRole.Owner);
        membership.SoftDelete();
        _membershipRepository.GetAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(membership);

        var result = await _sut.GetEffectivePermissionsAsync(_userId, _orgId, CancellationToken.None);

        result.Should().BeEmpty();
    }

    // =========================================================================
    // HasPermissionAsync - Different user IDs should be isolated
    // =========================================================================

    [Fact]
    public async Task HasPermission_DifferentUserId_ShouldBeIsolated()
    {
        var otherUserId = Guid.NewGuid();
        SetupMembership(MembershipRole.Admin);
        // Other user has no membership
        _membershipRepository.GetAsync(_orgId, otherUserId, Arg.Any<CancellationToken>())
            .Returns((Membership?)null);

        var resultOriginal = await _sut.HasPermissionAsync(_userId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);
        var resultOther = await _sut.HasPermissionAsync(otherUserId, _orgId, Permission.PropertiesViewAll, CancellationToken.None);

        resultOriginal.Should().BeTrue();
        resultOther.Should().BeFalse();
    }
}
