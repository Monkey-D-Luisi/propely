// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.OrgsApi.Domain.Permissions;
using Propely.OrgsApi.Domain.Permissions.Events;

namespace Propely.OrgsApi.UnitTests.Domain.Permissions;

public sealed class PermissionOverrideTests
{
    [Fact]
    public void Create_WithGrantedTrue_ShouldCreateOverrideAndRaiseGrantedEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var grantedBy = Guid.NewGuid();

        // Act
        var ov = PermissionOverride.Create(userId, orgId, Permission.PropertiesViewAll, true, grantedBy);

        // Assert
        ov.Id.Should().NotBeEmpty();
        ov.UserId.Should().Be(userId);
        ov.OrganizationId.Should().Be(orgId);
        ov.Permission.Should().Be(Permission.PropertiesViewAll);
        ov.Granted.Should().BeTrue();
        ov.GrantedBy.Should().Be(grantedBy);
        ov.GrantedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithGrantedTrue_ShouldRaisePermissionOverrideGrantedV1()
    {
        // Act
        var ov = PermissionOverride.Create(
            Guid.NewGuid(), Guid.NewGuid(), Permission.ContactsViewAll, true, Guid.NewGuid());

        // Assert
        ov.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PermissionOverrideGrantedV1>();
        var evt = (PermissionOverrideGrantedV1)ov.DomainEvents.First();
        evt.Data.OverrideId.Should().Be(ov.Id);
        evt.Data.UserId.Should().Be(ov.UserId);
        evt.Data.OrganizationId.Should().Be(ov.OrganizationId);
        evt.Data.Permission.Should().Be(Permission.ContactsViewAll);
        evt.Data.GrantedBy.Should().Be(ov.GrantedBy);
    }

    [Fact]
    public void Create_WithGrantedFalse_ShouldCreateOverrideAndRaiseDeniedEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var grantedBy = Guid.NewGuid();

        // Act
        var ov = PermissionOverride.Create(userId, orgId, Permission.PropertiesEditAll, false, grantedBy);

        // Assert
        ov.Id.Should().NotBeEmpty();
        ov.UserId.Should().Be(userId);
        ov.OrganizationId.Should().Be(orgId);
        ov.Permission.Should().Be(Permission.PropertiesEditAll);
        ov.Granted.Should().BeFalse();
        ov.GrantedBy.Should().Be(grantedBy);
    }

    [Fact]
    public void Create_WithGrantedFalse_ShouldRaisePermissionOverrideDeniedV1()
    {
        // Act
        var ov = PermissionOverride.Create(
            Guid.NewGuid(), Guid.NewGuid(), Permission.ReportsView, false, Guid.NewGuid());

        // Assert
        ov.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PermissionOverrideDeniedV1>();
        var evt = (PermissionOverrideDeniedV1)ov.DomainEvents.First();
        evt.Data.OverrideId.Should().Be(ov.Id);
        evt.Data.UserId.Should().Be(ov.UserId);
        evt.Data.OrganizationId.Should().Be(ov.OrganizationId);
        evt.Data.Permission.Should().Be(Permission.ReportsView);
    }

    [Fact]
    public void Revoke_ShouldRaisePermissionOverrideRevokedV1()
    {
        // Arrange
        var ov = PermissionOverride.Create(
            Guid.NewGuid(), Guid.NewGuid(), Permission.LeadsManage, true, Guid.NewGuid());
        ov.ClearDomainEvents();

        // Act
        ov.Revoke();

        // Assert
        ov.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PermissionOverrideRevokedV1>();
        var evt = (PermissionOverrideRevokedV1)ov.DomainEvents.First();
        evt.Data.OverrideId.Should().Be(ov.Id);
        evt.Data.UserId.Should().Be(ov.UserId);
        evt.Data.OrganizationId.Should().Be(ov.OrganizationId);
        evt.Data.Permission.Should().Be(Permission.LeadsManage);
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
    public void Create_WithEachPermission_ShouldSetPermissionCorrectly(Permission permission)
    {
        // Act
        var ov = PermissionOverride.Create(
            Guid.NewGuid(), Guid.NewGuid(), permission, true, Guid.NewGuid());

        // Assert
        ov.Permission.Should().Be(permission);
    }

    [Fact]
    public void Create_ShouldSetUniqueIds()
    {
        // Act
        var ov1 = PermissionOverride.Create(
            Guid.NewGuid(), Guid.NewGuid(), Permission.LeadsManage, true, Guid.NewGuid());
        var ov2 = PermissionOverride.Create(
            Guid.NewGuid(), Guid.NewGuid(), Permission.LeadsManage, true, Guid.NewGuid());

        // Assert
        ov1.Id.Should().NotBe(ov2.Id);
    }

    [Fact]
    public void DomainEvent_ShouldHaveCorrectMetadata()
    {
        // Act
        var ov = PermissionOverride.Create(
            Guid.NewGuid(), Guid.NewGuid(), Permission.PublishingManage, true, Guid.NewGuid());

        // Assert
        var evt = (PermissionOverrideGrantedV1)ov.DomainEvents.First();
        evt.EventId.Should().NotBeEmpty();
        evt.EventType.Should().Be(nameof(PermissionOverrideGrantedV1));
        evt.SchemaVersion.Should().Be(1);
        evt.Producer.Should().Be("OrgsApi");
        evt.OccurredAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
