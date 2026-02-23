// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.OrgsApi.Client.Dtos;
using Propely.OrgsApi.Client.Permissions;

namespace Propely.OrgsApi.UnitTests.Client;

public sealed class PermissionGuardTests
{
    private readonly IPermissionsApi _permissionsApi = Substitute.For<IPermissionsApi>();
    private readonly ILogger<PermissionGuard> _logger = Substitute.For<ILogger<PermissionGuard>>();
    private readonly PermissionGuard _sut;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _orgId = Guid.NewGuid();
    private const string Permission = "PropertiesViewAll";

    public PermissionGuardTests()
    {
        _sut = new PermissionGuard(_permissionsApi, _logger);
    }

    // --- HasPermissionAsync tests ---

    [Fact]
    public async Task HasPermissionAsync_WhenPermissionGranted_ReturnsTrue()
    {
        // Arrange
        _permissionsApi.GetEffectivePermissionsAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(new List<EffectivePermissionResponse>
            {
                new(Permission, true, "Role Default")
            });

        // Act
        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_WhenPermissionDenied_ReturnsFalse()
    {
        // Arrange
        _permissionsApi.GetEffectivePermissionsAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(new List<EffectivePermissionResponse>
            {
                new(Permission, false, "Override")
            });

        // Act
        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_WhenPermissionNotInList_ReturnsFalse()
    {
        // Arrange
        _permissionsApi.GetEffectivePermissionsAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(new List<EffectivePermissionResponse>
            {
                new("LeadsManage", true, "Role Default")
            });

        // Act
        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_WhenApiThrows_ReturnsFalse_FailClosed()
    {
        // Arrange
        _permissionsApi.GetEffectivePermissionsAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission);

        // Assert
        result.Should().BeFalse("fail-closed: unreachable API should deny access");
    }

    [Fact]
    public async Task HasPermissionAsync_WhenApiThrowsTaskCanceled_ReturnsFalse_FailClosed()
    {
        // Arrange
        _permissionsApi.GetEffectivePermissionsAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act
        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission);

        // Assert
        result.Should().BeFalse("fail-closed: timeout should deny access");
    }

    [Fact]
    public async Task HasPermissionAsync_IsCaseInsensitive()
    {
        // Arrange
        _permissionsApi.GetEffectivePermissionsAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(new List<EffectivePermissionResponse>
            {
                new("propertiesviewall", true, "Role Default")
            });

        // Act
        var result = await _sut.HasPermissionAsync(_userId, _orgId, "PropertiesViewAll");

        // Assert
        result.Should().BeTrue("permission comparison should be case-insensitive");
    }

    [Fact]
    public async Task HasPermissionAsync_WhenEmptyList_ReturnsFalse()
    {
        // Arrange
        _permissionsApi.GetEffectivePermissionsAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(new List<EffectivePermissionResponse>());

        // Act
        var result = await _sut.HasPermissionAsync(_userId, _orgId, Permission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_PropagatesCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _permissionsApi.GetEffectivePermissionsAsync(_orgId, _userId, token)
            .Returns(new List<EffectivePermissionResponse>
            {
                new(Permission, true, "Role Default")
            });

        // Act
        await _sut.HasPermissionAsync(_userId, _orgId, Permission, token);

        // Assert
        await _permissionsApi.Received(1).GetEffectivePermissionsAsync(_orgId, _userId, token);
    }

    // --- RequirePermissionAsync tests ---

    [Fact]
    public async Task RequirePermissionAsync_WhenPermissionGranted_DoesNotThrow()
    {
        // Arrange
        _permissionsApi.GetEffectivePermissionsAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(new List<EffectivePermissionResponse>
            {
                new(Permission, true, "Role Default")
            });

        // Act
        var act = () => _sut.RequirePermissionAsync(_userId, _orgId, Permission);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RequirePermissionAsync_WhenPermissionDenied_ThrowsForbiddenException()
    {
        // Arrange
        _permissionsApi.GetEffectivePermissionsAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(new List<EffectivePermissionResponse>
            {
                new(Permission, false, "Override")
            });

        // Act
        var act = () => _sut.RequirePermissionAsync(_userId, _orgId, Permission);

        // Assert
        var ex = await act.Should().ThrowAsync<ForbiddenException>();
        ex.Which.UserId.Should().Be(_userId);
        ex.Which.OrganizationId.Should().Be(_orgId);
        ex.Which.Permission.Should().Be(Permission);
    }

    [Fact]
    public async Task RequirePermissionAsync_WhenApiUnreachable_ThrowsForbiddenException_FailClosed()
    {
        // Arrange
        _permissionsApi.GetEffectivePermissionsAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var act = () => _sut.RequirePermissionAsync(_userId, _orgId, Permission);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>(
            "fail-closed: unreachable API should deny access via exception");
    }

    [Fact]
    public async Task RequirePermissionAsync_WhenPermissionNotInList_ThrowsForbiddenException()
    {
        // Arrange
        _permissionsApi.GetEffectivePermissionsAsync(_orgId, _userId, Arg.Any<CancellationToken>())
            .Returns(new List<EffectivePermissionResponse>());

        // Act
        var act = () => _sut.RequirePermissionAsync(_userId, _orgId, Permission);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    // --- Constructor tests ---

    [Fact]
    public void Constructor_WithNullPermissionsApi_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new PermissionGuard(null!, _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("permissionsApi");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new PermissionGuard(_permissionsApi, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}
