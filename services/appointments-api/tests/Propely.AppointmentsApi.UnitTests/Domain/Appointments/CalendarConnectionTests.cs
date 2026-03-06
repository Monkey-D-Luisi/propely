// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.AppointmentsApi.Domain.Appointments;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.UnitTests.Domain.Appointments;

public class CalendarConnectionTests
{
    private static readonly Guid ValidAgentId = Guid.NewGuid();
    private static readonly Guid ValidTenantId = Guid.NewGuid();

    private static CalendarConnection CreateValidConnection(
        Guid? agentId = null,
        Guid? tenantId = null,
        CalendarProvider provider = CalendarProvider.Google,
        string encryptedAccessToken = "encrypted-access-token",
        string encryptedRefreshToken = "encrypted-refresh-token",
        DateTime? tokenExpiresAtUtc = null,
        string externalCalendarId = "primary")
    {
        return CalendarConnection.Create(
            agentId ?? ValidAgentId,
            tenantId ?? ValidTenantId,
            provider,
            encryptedAccessToken,
            encryptedRefreshToken,
            tokenExpiresAtUtc ?? DateTime.UtcNow.AddHours(1),
            externalCalendarId);
    }

    // --- Create tests ---

    [Fact]
    public void Create_WithValidData_ReturnsActiveConnection()
    {
        var connection = CreateValidConnection();

        connection.Should().NotBeNull();
        connection.Id.Should().NotBeEmpty();
        connection.AgentId.Should().Be(ValidAgentId);
        connection.TenantId.Should().Be(ValidTenantId);
        connection.Provider.Should().Be(CalendarProvider.Google);
        connection.SyncState.Should().Be(CalendarSyncState.Active);
        connection.IsDeleted.Should().BeFalse();
        connection.LastSyncedUtc.Should().BeNull();
        connection.LastSyncError.Should().BeNull();
        connection.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithMicrosoftProvider_Succeeds()
    {
        var connection = CreateValidConnection(provider: CalendarProvider.Microsoft);

        connection.Provider.Should().Be(CalendarProvider.Microsoft);
    }

    [Fact]
    public void Create_WithEmptyAgentId_ThrowsDomainException()
    {
        var act = () => CreateValidConnection(agentId: Guid.Empty);

        act.Should().Throw<DomainException>()
            .WithMessage("*Agent ID*required*");
    }

    [Fact]
    public void Create_WithEmptyTenantId_ThrowsDomainException()
    {
        var act = () => CreateValidConnection(tenantId: Guid.Empty);

        act.Should().Throw<DomainException>()
            .WithMessage("*Tenant ID*required*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyAccessToken_ThrowsDomainException(string? token)
    {
        var act = () => CreateValidConnection(encryptedAccessToken: token!);

        act.Should().Throw<DomainException>()
            .WithMessage("*access token*required*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyRefreshToken_ThrowsDomainException(string? token)
    {
        var act = () => CreateValidConnection(encryptedRefreshToken: token!);

        act.Should().Throw<DomainException>()
            .WithMessage("*refresh token*required*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyExternalCalendarId_ThrowsDomainException(string? calendarId)
    {
        var act = () => CreateValidConnection(externalCalendarId: calendarId!);

        act.Should().Throw<DomainException>()
            .WithMessage("*External calendar ID*required*");
    }

    // --- UpdateTokens tests ---

    [Fact]
    public void UpdateTokens_WithValidData_UpdatesTokenFields()
    {
        var connection = CreateValidConnection();
        var newExpiry = DateTime.UtcNow.AddHours(2);

        connection.UpdateTokens("new-access", "new-refresh", newExpiry);

        connection.EncryptedAccessToken.Should().Be("new-access");
        connection.EncryptedRefreshToken.Should().Be("new-refresh");
        connection.TokenExpiresAtUtc.Should().Be(newExpiry);
        connection.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void UpdateTokens_WhenInErrorState_ResetsToActive()
    {
        var connection = CreateValidConnection();
        connection.MarkError("Some error");
        connection.SyncState.Should().Be(CalendarSyncState.Error);

        connection.UpdateTokens("new-access", "new-refresh", DateTime.UtcNow.AddHours(1));

        connection.SyncState.Should().Be(CalendarSyncState.Active);
        connection.LastSyncError.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateTokens_WithEmptyAccessToken_ThrowsDomainException(string? token)
    {
        var connection = CreateValidConnection();

        var act = () => connection.UpdateTokens(token!, "refresh", DateTime.UtcNow.AddHours(1));

        act.Should().Throw<DomainException>()
            .WithMessage("*access token*required*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateTokens_WithEmptyRefreshToken_ThrowsDomainException(string? token)
    {
        var connection = CreateValidConnection();

        var act = () => connection.UpdateTokens("access", token!, DateTime.UtcNow.AddHours(1));

        act.Should().Throw<DomainException>()
            .WithMessage("*refresh token*required*");
    }

    // --- MarkSynced tests ---

    [Fact]
    public void MarkSynced_SetsLastSyncedAndClearsError()
    {
        var connection = CreateValidConnection();
        connection.MarkError("Previous error");

        connection.MarkSynced();

        connection.LastSyncedUtc.Should().NotBeNull();
        connection.LastSyncedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        connection.LastSyncError.Should().BeNull();
        connection.SyncState.Should().Be(CalendarSyncState.Active);
        connection.UpdatedAtUtc.Should().NotBeNull();
    }

    // --- MarkError tests ---

    [Fact]
    public void MarkError_SetsErrorStateAndMessage()
    {
        var connection = CreateValidConnection();

        connection.MarkError("Token expired");

        connection.SyncState.Should().Be(CalendarSyncState.Error);
        connection.LastSyncError.Should().Be("Token expired");
        connection.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void MarkError_TruncatesLongErrorMessage()
    {
        var connection = CreateValidConnection();
        var longError = new string('A', CalendarConnection.LastSyncErrorMaxLength + 100);

        connection.MarkError(longError);

        connection.LastSyncError.Should().HaveLength(CalendarConnection.LastSyncErrorMaxLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MarkError_WithEmptyMessage_ThrowsDomainException(string? error)
    {
        var connection = CreateValidConnection();

        var act = () => connection.MarkError(error!);

        act.Should().Throw<DomainException>()
            .WithMessage("*Error message*required*");
    }

    // --- Disable tests ---

    [Fact]
    public void Disable_SetsStateToDisabled()
    {
        var connection = CreateValidConnection();

        connection.Disable();

        connection.SyncState.Should().Be(CalendarSyncState.Disabled);
        connection.UpdatedAtUtc.Should().NotBeNull();
    }

    // --- SoftDelete tests ---

    [Fact]
    public void SoftDelete_SetsIsDeletedAndDisablesSync()
    {
        var connection = CreateValidConnection();

        connection.SoftDelete();

        connection.IsDeleted.Should().BeTrue();
        connection.DeletedAtUtc.Should().NotBeNull();
        connection.DeletedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        connection.SyncState.Should().Be(CalendarSyncState.Disabled);
        connection.UpdatedAtUtc.Should().NotBeNull();
    }
}
