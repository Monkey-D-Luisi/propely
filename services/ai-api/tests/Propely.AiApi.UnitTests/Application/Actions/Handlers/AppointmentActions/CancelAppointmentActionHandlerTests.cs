// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.AiApi.Application.Actions.Commands.AppointmentActions;
using Propely.AiApi.Application.Actions.Handlers.AppointmentActions;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;
using Propely.AppointmentsApi.Client;
using Propely.AppointmentsApi.Client.Models;

namespace Propely.AiApi.UnitTests.Application.Actions.Handlers.AppointmentActions;

public sealed class CancelAppointmentActionHandlerTests
{
    private readonly IAppointmentsApiClient _appointmentsClient;
    private readonly CancelAppointmentActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public CancelAppointmentActionHandlerTests()
    {
        _appointmentsClient = Substitute.For<IAppointmentsApiClient>();
        var logger = Substitute.For<ILogger<CancelAppointmentActionHandler>>();
        _handler = new CancelAppointmentActionHandler(_appointmentsClient, logger);
    }

    [Fact]
    public async Task Handle_WithValidParameters_ShouldCancelAppointmentViaSDK()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var parameters = new CancelAppointmentParameters(
            AppointmentId: appointmentId,
            Reason: "Client requested cancellation");
        var command = new CancelAppointmentActionCommand(parameters, TenantId, AgentId);

        var cancelledAppointment = new AppointmentResponse
        {
            Id = appointmentId,
            Title = "Viewing at Calle Mayor",
            StartTimeUtc = new DateTime(2026, 3, 15, 10, 0, 0),
            Status = "Cancelled"
        };

        _appointmentsClient.CancelAppointmentAsync(
            appointmentId,
            Arg.Any<CancelAppointmentClientRequest>(),
            Arg.Any<CancellationToken>())
            .Returns(cancelledAppointment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.CancelAppointment);
        result.Message.Should().Contain("Viewing at Calle Mayor");
        result.Message.Should().Contain("cancelled");
        await _appointmentsClient.Received(1).CancelAppointmentAsync(
            appointmentId,
            Arg.Is<CancelAppointmentClientRequest>(r => r.Reason == "Client requested cancellation"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMissingAppointmentId_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new CancelAppointmentParameters(
            AppointmentId: null,
            Reason: "No longer needed");
        var command = new CancelAppointmentActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.CancelAppointment);
        result.Errors.Should().Contain("Appointment ID is required to cancel an appointment.");
    }

    [Fact]
    public async Task Handle_WhenSdkThrows_ShouldReturnFailureGracefully()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var parameters = new CancelAppointmentParameters(
            AppointmentId: appointmentId,
            Reason: null);
        var command = new CancelAppointmentActionCommand(parameters, TenantId, AgentId);

        _appointmentsClient.CancelAppointmentAsync(
            appointmentId,
            Arg.Any<CancelAppointmentClientRequest>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.CancelAppointment);
        result.Errors.Should().NotBeEmpty();
    }
}
