// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.OrgsApi.Domain.Organizations;
using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.UnitTests.Domain.Permissions;

public sealed class DefaultPermissionMatrixTests
{
    [Fact]
    public void GetDefaults_Owner_ShouldReturnAllPermissions()
    {
        // Act
        var defaults = DefaultPermissionMatrix.GetDefaults(MembershipRole.Owner);

        // Assert
        var allPermissions = Enum.GetValues<Permission>();
        defaults.Should().BeEquivalentTo(allPermissions);
    }

    [Fact]
    public void GetDefaults_Admin_ShouldReturnAllPermissions()
    {
        // Act
        var defaults = DefaultPermissionMatrix.GetDefaults(MembershipRole.Admin);

        // Assert
        var allPermissions = Enum.GetValues<Permission>();
        defaults.Should().BeEquivalentTo(allPermissions);
    }

    [Fact]
    public void GetDefaults_Agent_ShouldReturnOnlyLeadsManage()
    {
        // Act
        var defaults = DefaultPermissionMatrix.GetDefaults(MembershipRole.Agent);

        // Assert
        defaults.Should().ContainSingle()
            .Which.Should().Be(Permission.LeadsManage);
    }

    [Fact]
    public void GetDefaults_Viewer_ShouldReturnEmptySet()
    {
        // Act
        var defaults = DefaultPermissionMatrix.GetDefaults(MembershipRole.Viewer);

        // Assert
        defaults.Should().BeEmpty();
    }

    [Theory]
    [InlineData(Permission.PropertiesViewAll)]
    [InlineData(Permission.PropertiesEditAll)]
    [InlineData(Permission.ContactsViewAll)]
    [InlineData(Permission.ContactsEditAll)]
    [InlineData(Permission.AppointmentsViewAll)]
    [InlineData(Permission.PublishingManage)]
    [InlineData(Permission.LeadsManage)]
    [InlineData(Permission.ReportsView)]
    public void GetDefaults_Owner_ShouldContainEveryPermission(Permission permission)
    {
        // Act
        var defaults = DefaultPermissionMatrix.GetDefaults(MembershipRole.Owner);

        // Assert
        defaults.Should().Contain(permission);
    }

    [Theory]
    [InlineData(Permission.PropertiesViewAll)]
    [InlineData(Permission.PropertiesEditAll)]
    [InlineData(Permission.ContactsViewAll)]
    [InlineData(Permission.ContactsEditAll)]
    [InlineData(Permission.AppointmentsViewAll)]
    [InlineData(Permission.PublishingManage)]
    [InlineData(Permission.LeadsManage)]
    [InlineData(Permission.ReportsView)]
    public void GetDefaults_Admin_ShouldContainEveryPermission(Permission permission)
    {
        // Act
        var defaults = DefaultPermissionMatrix.GetDefaults(MembershipRole.Admin);

        // Assert
        defaults.Should().Contain(permission);
    }

    [Theory]
    [InlineData(Permission.PropertiesViewAll)]
    [InlineData(Permission.PropertiesEditAll)]
    [InlineData(Permission.ContactsViewAll)]
    [InlineData(Permission.ContactsEditAll)]
    [InlineData(Permission.AppointmentsViewAll)]
    [InlineData(Permission.PublishingManage)]
    [InlineData(Permission.ReportsView)]
    public void GetDefaults_Agent_ShouldNotContainNonDefaultPermissions(Permission permission)
    {
        // Act
        var defaults = DefaultPermissionMatrix.GetDefaults(MembershipRole.Agent);

        // Assert
        defaults.Should().NotContain(permission);
    }

    [Theory]
    [InlineData(Permission.PropertiesViewAll)]
    [InlineData(Permission.PropertiesEditAll)]
    [InlineData(Permission.ContactsViewAll)]
    [InlineData(Permission.ContactsEditAll)]
    [InlineData(Permission.AppointmentsViewAll)]
    [InlineData(Permission.PublishingManage)]
    [InlineData(Permission.LeadsManage)]
    [InlineData(Permission.ReportsView)]
    public void GetDefaults_Viewer_ShouldNotContainAnyPermission(Permission permission)
    {
        // Act
        var defaults = DefaultPermissionMatrix.GetDefaults(MembershipRole.Viewer);

        // Assert
        defaults.Should().NotContain(permission);
    }

    [Fact]
    public void GetDefaults_InvalidRole_ShouldThrowArgumentOutOfRange()
    {
        // Act
        var act = () => DefaultPermissionMatrix.GetDefaults((MembershipRole)999);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetDefaults_ShouldReturnReadOnlySet()
    {
        // Act
        var defaults = DefaultPermissionMatrix.GetDefaults(MembershipRole.Owner);

        // Assert
        defaults.Should().BeAssignableTo<IReadOnlySet<Permission>>();
    }
}
