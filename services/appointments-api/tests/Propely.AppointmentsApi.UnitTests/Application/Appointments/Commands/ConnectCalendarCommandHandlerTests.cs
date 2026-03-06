// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AppointmentsApi.Application.Appointments.Commands.ConnectCalendar;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Domain.Appointments;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.UnitTests.Application.Appointments.Commands;

public class ConnectCalendarCommandHandlerTests
{
    private readonly ICalendarTokenExchangeService _tokenExchangeService = Substitute.For<ICalendarTokenExchangeService>();
    private readonly ITokenEncryptionService _encryptionService = Substitute.For<ITokenEncryptionService>();
    private readonly ICalendarConnectionRepository _connectionRepository = Substitute.For<ICalendarConnectionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ConnectCalendarCommandHandler _handler;

    private static readonly Guid AgentId = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();

    public ConnectCalendarCommandHandlerTests()
    {
        _handler = new ConnectCalendarCommandHandler(
            _tokenExchangeService, _encryptionService, _connectionRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesConnectionAndReturnsDto()
    {
        var command = new ConnectCalendarCommand
        {
            AgentId = AgentId,
            TenantId = TenantId,
            Provider = CalendarProvider.Google,
            AuthorizationCode = "auth-code-123",
            RedirectUri = "https://app.propely.com/callback"
        };

        var tokenResult = new CalendarTokenResult(
            "access-token", "refresh-token", DateTime.UtcNow.AddHours(1), "primary");

        _connectionRepository.GetByAgentAndProviderAsync(
            AgentId, TenantId, CalendarProvider.Google, Arg.Any<CancellationToken>())
            .Returns((CalendarConnection?)null);

        _tokenExchangeService.ExchangeCodeAsync(
            CalendarProvider.Google, "auth-code-123", "https://app.propely.com/callback", Arg.Any<CancellationToken>())
            .Returns(tokenResult);

        _encryptionService.Encrypt("access-token").Returns("encrypted-access");
        _encryptionService.Encrypt("refresh-token").Returns("encrypted-refresh");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.AgentId.Should().Be(AgentId);
        result.Provider.Should().Be(CalendarProvider.Google);
        result.SyncState.Should().Be(CalendarSyncState.Active);
        result.ExternalCalendarId.Should().Be("primary");

        await _connectionRepository.Received(1).AddAsync(Arg.Any<CalendarConnection>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingConnection_ThrowsConflictException()
    {
        var command = new ConnectCalendarCommand
        {
            AgentId = AgentId,
            TenantId = TenantId,
            Provider = CalendarProvider.Google,
            AuthorizationCode = "auth-code-123",
            RedirectUri = "https://app.propely.com/callback"
        };

        var existingConnection = CalendarConnection.Create(
            AgentId, TenantId, CalendarProvider.Google, "token", "refresh", DateTime.UtcNow.AddHours(1), "primary");

        _connectionRepository.GetByAgentAndProviderAsync(
            AgentId, TenantId, CalendarProvider.Google, Arg.Any<CancellationToken>())
            .Returns(existingConnection);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Google*already exists*");
    }

    [Fact]
    public async Task Handle_MicrosoftProvider_CreatesConnectionCorrectly()
    {
        var command = new ConnectCalendarCommand
        {
            AgentId = AgentId,
            TenantId = TenantId,
            Provider = CalendarProvider.Microsoft,
            AuthorizationCode = "ms-auth-code",
            RedirectUri = "https://app.propely.com/ms-callback"
        };

        var tokenResult = new CalendarTokenResult(
            "ms-access", "ms-refresh", DateTime.UtcNow.AddHours(1), "user@outlook.com");

        _connectionRepository.GetByAgentAndProviderAsync(
            AgentId, TenantId, CalendarProvider.Microsoft, Arg.Any<CancellationToken>())
            .Returns((CalendarConnection?)null);

        _tokenExchangeService.ExchangeCodeAsync(
            CalendarProvider.Microsoft, "ms-auth-code", "https://app.propely.com/ms-callback", Arg.Any<CancellationToken>())
            .Returns(tokenResult);

        _encryptionService.Encrypt("ms-access").Returns("encrypted-ms-access");
        _encryptionService.Encrypt("ms-refresh").Returns("encrypted-ms-refresh");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Provider.Should().Be(CalendarProvider.Microsoft);
        result.ExternalCalendarId.Should().Be("user@outlook.com");
    }
}
