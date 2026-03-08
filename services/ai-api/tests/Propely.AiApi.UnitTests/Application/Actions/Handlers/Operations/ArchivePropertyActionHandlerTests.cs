// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.AiApi.Application.Actions.Commands.Operations;
using Propely.AiApi.Application.Actions.Handlers.Operations;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.UnitTests.Application.Actions.Handlers.Operations;

public sealed class ArchivePropertyActionHandlerTests
{
    private readonly ArchivePropertyActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public ArchivePropertyActionHandlerTests()
    {
        var logger = Substitute.For<ILogger<ArchivePropertyActionHandler>>();
        _handler = new ArchivePropertyActionHandler(logger);
    }

    [Fact]
    public async Task Handle_WhenPropertyIdProvided_ShouldReturnSuccess()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new ArchivePropertyParameters(
            PropertyId: propertyId,
            Reference: null);
        var command = new ArchivePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.ArchiveProperty);
        result.Message.Should().Contain(propertyId.ToString());
        result.Message.ToLowerInvariant().Should().Contain("archive");
    }

    [Fact]
    public async Task Handle_WhenReferenceProvided_ShouldReturnSuccess()
    {
        // Arrange
        var parameters = new ArchivePropertyParameters(
            PropertyId: null,
            Reference: "AP-2024-015");
        var command = new ArchivePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.ArchiveProperty);
        result.Message.Should().Contain("AP-2024-015");
    }

    [Fact]
    public async Task Handle_WhenMissingPropertyIdentifier_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new ArchivePropertyParameters(
            PropertyId: null,
            Reference: null);
        var command = new ArchivePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ArchiveProperty);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("property");
    }

    [Fact]
    public async Task Handle_WhenValidParameters_ShouldIncludeStructuredData()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new ArchivePropertyParameters(
            PropertyId: propertyId,
            Reference: null);
        var command = new ArchivePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var data = result.Data as Dictionary<string, object?>;
        data.Should().NotBeNull();
        data!["propertyId"].Should().Be(propertyId);
        data["targetStatus"].Should().Be("Archived");
        data["agentId"].Should().Be(AgentId);
        data["tenantId"].Should().Be(TenantId);
    }

    [Fact]
    public async Task Handle_WhenBothIdAndReferenceProvided_ShouldUsePropertyId()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var parameters = new ArchivePropertyParameters(
            PropertyId: propertyId,
            Reference: "AP-2024-015");
        var command = new ArchivePropertyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain(propertyId.ToString());
    }
}
