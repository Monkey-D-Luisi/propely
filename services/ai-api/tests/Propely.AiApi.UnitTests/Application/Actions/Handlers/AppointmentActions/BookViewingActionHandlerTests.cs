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

public sealed class BookViewingActionHandlerTests
{
    private readonly IAppointmentsApiClient _appointmentsClient;
    private readonly BookViewingActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public BookViewingActionHandlerTests()
    {
        _appointmentsClient = Substitute.For<IAppointmentsApiClient>();
        var logger = Substitute.For<ILogger<BookViewingActionHandler>>();
        _handler = new BookViewingActionHandler(_appointmentsClient, logger);
    }

    [Fact]
    public async Task Handle_WithValidParameters_ShouldCreateAppointmentViaSDK()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var parameters = new BookViewingParameters(
            PropertyId: propertyId,
            ContactId: contactId,
            StartTime: new DateTime(2026, 3, 15, 10, 0, 0),
            EndTime: null,
            Title: "Viewing at Calle Mayor",
            Location: "Calle Mayor 5, Madrid",
            Notes: null);
        var command = new BookViewingActionCommand(parameters, TenantId, AgentId);

        var createdAppointment = new AppointmentResponse
        {
            Id = Guid.NewGuid(),
            Title = "Viewing at Calle Mayor",
            StartTimeUtc = new DateTime(2026, 3, 15, 10, 0, 0),
            EndTimeUtc = new DateTime(2026, 3, 15, 10, 30, 0),
            PropertyId = propertyId,
            ContactId = contactId,
            Status = "Scheduled"
        };

        _appointmentsClient.CreateAppointmentAsync(Arg.Any<CreateAppointmentClientRequest>(), Arg.Any<CancellationToken>())
            .Returns(createdAppointment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.BookViewing);
        result.Message.Should().Contain("2026-03-15");
        await _appointmentsClient.Received(1).CreateAppointmentAsync(
            Arg.Is<CreateAppointmentClientRequest>(r =>
                r.PropertyId == propertyId &&
                r.ContactId == contactId &&
                r.Type == "PropertyViewing" &&
                r.Title == "Viewing at Calle Mayor"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMissingPropertyId_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new BookViewingParameters(
            PropertyId: null,
            ContactId: null,
            StartTime: new DateTime(2026, 3, 15, 10, 0, 0),
            EndTime: null,
            Title: null,
            Location: null,
            Notes: null);
        var command = new BookViewingActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.BookViewing);
        result.Errors.Should().Contain("A property ID is required to book a viewing.");
    }

    [Fact]
    public async Task Handle_WithMissingStartTime_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new BookViewingParameters(
            PropertyId: Guid.NewGuid(),
            ContactId: null,
            StartTime: null,
            EndTime: null,
            Title: null,
            Location: null,
            Notes: null);
        var command = new BookViewingActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.BookViewing);
        result.Errors.Should().Contain("A valid start time is required to book a viewing.");
    }

    [Fact]
    public async Task Handle_WithInvalidStartTime_ShouldReturnFailure()
    {
        // Arrange — DateTime? is null when not parseable; handler should treat null as missing
        var parameters = new BookViewingParameters(
            PropertyId: Guid.NewGuid(),
            ContactId: null,
            StartTime: null,
            EndTime: null,
            Title: null,
            Location: null,
            Notes: null);
        var command = new BookViewingActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.BookViewing);
        result.Errors.Should().Contain("A valid start time is required to book a viewing.");
    }

    [Fact]
    public async Task Handle_WithoutTitle_ShouldDefaultToPropertyViewing()
    {
        // Arrange
        var parameters = new BookViewingParameters(
            PropertyId: Guid.NewGuid(),
            ContactId: null,
            StartTime: new DateTime(2026, 3, 15, 10, 0, 0),
            EndTime: null,
            Title: null,
            Location: null,
            Notes: null);
        var command = new BookViewingActionCommand(parameters, TenantId, AgentId);

        _appointmentsClient.CreateAppointmentAsync(Arg.Any<CreateAppointmentClientRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AppointmentResponse
            {
                Id = Guid.NewGuid(),
                Title = "Property Viewing",
                StartTimeUtc = new DateTime(2026, 3, 15, 10, 0, 0),
                EndTimeUtc = new DateTime(2026, 3, 15, 10, 30, 0),
                Status = "Scheduled"
            });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _appointmentsClient.Received(1).CreateAppointmentAsync(
            Arg.Is<CreateAppointmentClientRequest>(r => r.Title == "Property Viewing"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithoutEndTime_ShouldDefault30Minutes()
    {
        // Arrange
        var parameters = new BookViewingParameters(
            PropertyId: Guid.NewGuid(),
            ContactId: null,
            StartTime: new DateTime(2026, 3, 15, 10, 0, 0),
            EndTime: null,
            Title: null,
            Location: null,
            Notes: null);
        var command = new BookViewingActionCommand(parameters, TenantId, AgentId);

        _appointmentsClient.CreateAppointmentAsync(Arg.Any<CreateAppointmentClientRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AppointmentResponse
            {
                Id = Guid.NewGuid(),
                Title = "Property Viewing",
                StartTimeUtc = new DateTime(2026, 3, 15, 10, 0, 0),
                EndTimeUtc = new DateTime(2026, 3, 15, 10, 30, 0),
                Status = "Scheduled"
            });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _appointmentsClient.Received(1).CreateAppointmentAsync(
            Arg.Is<CreateAppointmentClientRequest>(r =>
                r.StartTimeUtc == new DateTime(2026, 3, 15, 10, 0, 0) &&
                r.EndTimeUtc == new DateTime(2026, 3, 15, 10, 30, 0)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSdkThrows_ShouldReturnFailureGracefully()
    {
        // Arrange
        var parameters = new BookViewingParameters(
            PropertyId: Guid.NewGuid(),
            ContactId: null,
            StartTime: new DateTime(2026, 3, 15, 10, 0, 0),
            EndTime: null,
            Title: null,
            Location: null,
            Notes: null);
        var command = new BookViewingActionCommand(parameters, TenantId, AgentId);

        _appointmentsClient.CreateAppointmentAsync(Arg.Any<CreateAppointmentClientRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.BookViewing);
        result.Errors.Should().NotBeEmpty();
    }
}
