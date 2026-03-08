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

public sealed class RescheduleAppointmentActionHandlerTests
{
    private readonly IAppointmentsApiClient _appointmentsClient;
    private readonly RescheduleAppointmentActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public RescheduleAppointmentActionHandlerTests()
    {
        _appointmentsClient = Substitute.For<IAppointmentsApiClient>();
        var logger = Substitute.For<ILogger<RescheduleAppointmentActionHandler>>();
        _handler = new RescheduleAppointmentActionHandler(_appointmentsClient, logger);
    }

    [Fact]
    public async Task Handle_WithValidParameters_ShouldRescheduleAppointmentViaSDK()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var parameters = new RescheduleAppointmentParameters(
            AppointmentId: appointmentId,
            NewStartTime: new DateTime(2026, 3, 20, 14, 0, 0),
            NewEndTime: null);
        var command = new RescheduleAppointmentActionCommand(parameters, TenantId, AgentId);

        var existingAppointment = new AppointmentResponse
        {
            Id = appointmentId,
            Title = "Viewing at Calle Mayor",
            Type = "PropertyViewing",
            StartTimeUtc = new DateTime(2026, 3, 15, 10, 0, 0),
            EndTimeUtc = new DateTime(2026, 3, 15, 10, 30, 0),
            PropertyId = propertyId,
            Status = "Scheduled"
        };

        var updatedAppointment = new AppointmentResponse
        {
            Id = appointmentId,
            Title = "Viewing at Calle Mayor",
            StartTimeUtc = new DateTime(2026, 3, 20, 14, 0, 0),
            EndTimeUtc = new DateTime(2026, 3, 20, 14, 30, 0),
            Status = "Scheduled"
        };

        _appointmentsClient.GetAppointmentByIdAsync(appointmentId, Arg.Any<CancellationToken>())
            .Returns(existingAppointment);
        _appointmentsClient.UpdateAppointmentAsync(appointmentId, Arg.Any<UpdateAppointmentClientRequest>(), Arg.Any<CancellationToken>())
            .Returns(updatedAppointment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.RescheduleAppointment);
        result.Message.Should().Contain("Rescheduled");
        result.Message.Should().Contain("2026-03-20");
        await _appointmentsClient.Received(1).UpdateAppointmentAsync(
            appointmentId,
            Arg.Is<UpdateAppointmentClientRequest>(r =>
                r.StartTimeUtc == new DateTime(2026, 3, 20, 14, 0, 0) &&
                r.EndTimeUtc == new DateTime(2026, 3, 20, 14, 30, 0) &&
                r.Title == "Viewing at Calle Mayor"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMissingAppointmentId_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new RescheduleAppointmentParameters(
            AppointmentId: null,
            NewStartTime: new DateTime(2026, 3, 20, 14, 0, 0),
            NewEndTime: null);
        var command = new RescheduleAppointmentActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.RescheduleAppointment);
        result.Errors.Should().Contain("Appointment ID is required to reschedule.");
    }

    [Fact]
    public async Task Handle_WithMissingNewStartTime_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new RescheduleAppointmentParameters(
            AppointmentId: Guid.NewGuid(),
            NewStartTime: null,
            NewEndTime: null);
        var command = new RescheduleAppointmentActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.RescheduleAppointment);
        result.Errors.Should().Contain("A valid new start time is required to reschedule.");
    }

    [Fact]
    public async Task Handle_WithInvalidNewStartTime_ShouldReturnFailure()
    {
        // Arrange — DateTime? is null when not parseable; handler should treat null as missing
        var parameters = new RescheduleAppointmentParameters(
            AppointmentId: Guid.NewGuid(),
            NewStartTime: null,
            NewEndTime: null);
        var command = new RescheduleAppointmentActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.RescheduleAppointment);
        result.Errors.Should().Contain("A valid new start time is required to reschedule.");
    }

    [Fact]
    public async Task Handle_ShouldPreserveOriginalDuration()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var parameters = new RescheduleAppointmentParameters(
            AppointmentId: appointmentId,
            NewStartTime: new DateTime(2026, 3, 20, 14, 0, 0),
            NewEndTime: null);
        var command = new RescheduleAppointmentActionCommand(parameters, TenantId, AgentId);

        // Original is 1 hour duration
        var existing = new AppointmentResponse
        {
            Id = appointmentId,
            Title = "Long Viewing",
            Type = "PropertyViewing",
            StartTimeUtc = new DateTime(2026, 3, 15, 10, 0, 0),
            EndTimeUtc = new DateTime(2026, 3, 15, 11, 0, 0),
            Status = "Scheduled"
        };

        _appointmentsClient.GetAppointmentByIdAsync(appointmentId, Arg.Any<CancellationToken>())
            .Returns(existing);
        _appointmentsClient.UpdateAppointmentAsync(appointmentId, Arg.Any<UpdateAppointmentClientRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AppointmentResponse
            {
                Id = appointmentId,
                Title = "Long Viewing",
                StartTimeUtc = new DateTime(2026, 3, 20, 14, 0, 0),
                EndTimeUtc = new DateTime(2026, 3, 20, 15, 0, 0),
                Status = "Scheduled"
            });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — new end time should preserve the original 1-hour duration
        await _appointmentsClient.Received(1).UpdateAppointmentAsync(
            appointmentId,
            Arg.Is<UpdateAppointmentClientRequest>(r =>
                r.EndTimeUtc == new DateTime(2026, 3, 20, 15, 0, 0)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEndTimeBeforeStartTime_ShouldReturnFailure()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var parameters = new RescheduleAppointmentParameters(
            AppointmentId: appointmentId,
            NewStartTime: new DateTime(2026, 3, 20, 14, 0, 0),
            NewEndTime: new DateTime(2026, 3, 20, 13, 0, 0));
        var command = new RescheduleAppointmentActionCommand(parameters, TenantId, AgentId);

        var existing = new AppointmentResponse
        {
            Id = appointmentId,
            Title = "Test",
            Type = "PropertyViewing",
            StartTimeUtc = new DateTime(2026, 3, 15, 10, 0, 0),
            EndTimeUtc = new DateTime(2026, 3, 15, 10, 30, 0),
            Status = "Scheduled"
        };

        _appointmentsClient.GetAppointmentByIdAsync(appointmentId, Arg.Any<CancellationToken>())
            .Returns(existing);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.RescheduleAppointment);
        result.Errors.Should().Contain("The new end time must be after the new start time.");
    }

    [Fact]
    public async Task Handle_ShouldPreserveIsAllDayFlag()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var parameters = new RescheduleAppointmentParameters(
            AppointmentId: appointmentId,
            NewStartTime: new DateTime(2026, 3, 20, 14, 0, 0),
            NewEndTime: null);
        var command = new RescheduleAppointmentActionCommand(parameters, TenantId, AgentId);

        var existing = new AppointmentResponse
        {
            Id = appointmentId,
            Title = "All Day Event",
            Type = "PropertyViewing",
            StartTimeUtc = new DateTime(2026, 3, 15, 0, 0, 0),
            EndTimeUtc = new DateTime(2026, 3, 15, 23, 59, 0),
            IsAllDay = true,
            Status = "Scheduled"
        };

        _appointmentsClient.GetAppointmentByIdAsync(appointmentId, Arg.Any<CancellationToken>())
            .Returns(existing);
        _appointmentsClient.UpdateAppointmentAsync(appointmentId, Arg.Any<UpdateAppointmentClientRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AppointmentResponse
            {
                Id = appointmentId,
                Title = "All Day Event",
                StartTimeUtc = new DateTime(2026, 3, 20, 14, 0, 0),
                EndTimeUtc = new DateTime(2026, 3, 21, 13, 59, 0),
                IsAllDay = true,
                Status = "Scheduled"
            });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — IsAllDay should be preserved
        await _appointmentsClient.Received(1).UpdateAppointmentAsync(
            appointmentId,
            Arg.Is<UpdateAppointmentClientRequest>(r => r.IsAllDay == true),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenFetchFails_ShouldReturnFailure()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var parameters = new RescheduleAppointmentParameters(
            AppointmentId: appointmentId,
            NewStartTime: new DateTime(2026, 3, 20, 14, 0, 0),
            NewEndTime: null);
        var command = new RescheduleAppointmentActionCommand(parameters, TenantId, AgentId);

        _appointmentsClient.GetAppointmentByIdAsync(appointmentId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Not found"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.RescheduleAppointment);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUpdateFails_ShouldReturnFailure()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var parameters = new RescheduleAppointmentParameters(
            AppointmentId: appointmentId,
            NewStartTime: new DateTime(2026, 3, 20, 14, 0, 0),
            NewEndTime: null);
        var command = new RescheduleAppointmentActionCommand(parameters, TenantId, AgentId);

        _appointmentsClient.GetAppointmentByIdAsync(appointmentId, Arg.Any<CancellationToken>())
            .Returns(new AppointmentResponse
            {
                Id = appointmentId,
                Title = "Test",
                Type = "PropertyViewing",
                StartTimeUtc = new DateTime(2026, 3, 15, 10, 0, 0),
                EndTimeUtc = new DateTime(2026, 3, 15, 10, 30, 0),
                Status = "Scheduled"
            });
        _appointmentsClient.UpdateAppointmentAsync(appointmentId, Arg.Any<UpdateAppointmentClientRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.RescheduleAppointment);
        result.Errors.Should().NotBeEmpty();
    }
}
