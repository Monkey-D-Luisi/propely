// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.AiApi.Application.Actions.Commands.ContactActions;
using Propely.AiApi.Application.Actions.Handlers.ContactActions;
using Propely.AiApi.Domain.Actions;
using Propely.ContactsApi.Client;
using Propely.ContactsApi.Client.Models;

namespace Propely.AiApi.UnitTests.Application.Actions.Handlers.ContactActions;

public sealed class CreateLeadActionHandlerTests
{
    private readonly ILeadsApiClient _leadsClient;
    private readonly CreateLeadActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public CreateLeadActionHandlerTests()
    {
        _leadsClient = Substitute.For<ILeadsApiClient>();
        var logger = Substitute.For<ILogger<CreateLeadActionHandler>>();
        _handler = new CreateLeadActionHandler(_leadsClient, logger);
    }

    [Fact]
    public async Task Handle_WithValidParameters_ShouldCreateLeadViaSDK()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new Dictionary<string, object?>
        {
            ["name"] = "Maria Garcia",
            ["email"] = "maria@example.com",
            ["phone"] = "650123456",
            ["property_id"] = propertyId.ToString()
        };
        var command = new CreateLeadActionCommand(parameters, TenantId, AgentId);

        var createdLead = new LeadResponse
        {
            Id = Guid.NewGuid(),
            Name = "Maria Garcia",
            Email = "maria@example.com",
            Phone = "650123456",
            PropertyId = propertyId,
            Status = "New"
        };

        _leadsClient.CreateLeadAsync(Arg.Any<CreateLeadClientRequest>(), Arg.Any<CancellationToken>())
            .Returns(createdLead);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.CreateLead);
        result.Message.Should().Contain("Maria Garcia");
        result.Message.Should().Contain("maria@example.com");
        await _leadsClient.Received(1).CreateLeadAsync(
            Arg.Is<CreateLeadClientRequest>(r =>
                r.Name == "Maria Garcia" &&
                r.Email == "maria@example.com" &&
                r.PropertyId == propertyId &&
                r.AssignedAgentId == AgentId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMissingName_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["email"] = "test@example.com",
            ["property_id"] = Guid.NewGuid().ToString()
        };
        var command = new CreateLeadActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.CreateLead);
        result.Errors.Should().Contain("Lead name is required.");
    }

    [Fact]
    public async Task Handle_WithMissingEmail_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["name"] = "Test Lead",
            ["property_id"] = Guid.NewGuid().ToString()
        };
        var command = new CreateLeadActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.CreateLead);
        result.Errors.Should().Contain("Lead email is required.");
    }

    [Fact]
    public async Task Handle_WithMissingPropertyId_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["name"] = "Test Lead",
            ["email"] = "test@example.com"
        };
        var command = new CreateLeadActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.CreateLead);
        result.Errors.Should().Contain("A property ID is required for a lead.");
    }

    [Fact]
    public async Task Handle_WhenSdkThrows_ShouldReturnFailureGracefully()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["name"] = "Test Lead",
            ["email"] = "test@example.com",
            ["property_id"] = Guid.NewGuid().ToString()
        };
        var command = new CreateLeadActionCommand(parameters, TenantId, AgentId);

        _leadsClient.CreateLeadAsync(Arg.Any<CreateLeadClientRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.CreateLead);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldSetSourceToNaturalLanguage()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["name"] = "Test",
            ["email"] = "test@example.com",
            ["property_id"] = Guid.NewGuid().ToString()
        };
        var command = new CreateLeadActionCommand(parameters, TenantId, AgentId);

        _leadsClient.CreateLeadAsync(Arg.Any<CreateLeadClientRequest>(), Arg.Any<CancellationToken>())
            .Returns(new LeadResponse { Id = Guid.NewGuid(), Name = "Test", Email = "test@example.com", Status = "New" });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _leadsClient.Received(1).CreateLeadAsync(
            Arg.Is<CreateLeadClientRequest>(r => r.Source == "NaturalLanguage"),
            Arg.Any<CancellationToken>());
    }
}
