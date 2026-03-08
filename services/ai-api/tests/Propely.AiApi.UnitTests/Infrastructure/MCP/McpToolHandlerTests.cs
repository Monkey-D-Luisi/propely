// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.AiApi.Application.Actions.Interfaces;
using Propely.AiApi.Application.Actions.Tools;
using Propely.AiApi.Domain.Actions;
using Propely.AiApi.Infrastructure.MCP;

namespace Propely.AiApi.UnitTests.Infrastructure.MCP;

public sealed class McpToolHandlerTests
{
    private readonly IActionRouter _router;
    private readonly McpToolHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public McpToolHandlerTests()
    {
        _router = Substitute.For<IActionRouter>();
        var registry = new ToolSchemaRegistry();
        var logger = Substitute.For<ILogger<McpToolHandler>>();
        _handler = new McpToolHandler(registry, _router, logger);
    }

    [Fact]
    public async Task HandleToolCallAsync_WithValidTool_ShouldRouteToActionRouter()
    {
        // Arrange
        var arguments = new Dictionary<string, object?>
        {
            ["title"] = "Test Property",
            ["city"] = "Madrid"
        };
        var expectedResult = ActionResult.Ok(
            data: new { id = Guid.NewGuid() },
            message: "Property created successfully.",
            type: ActionType.CreateProperty);

        _router.RouteAsync(
            Arg.Any<ClassifiedIntent>(),
            TenantId,
            AgentId,
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.HandleToolCallAsync(
            "create_property", arguments, TenantId, AgentId, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Property created successfully.");
        result.ActionType.Should().Be(ActionType.CreateProperty);

        await _router.Received(1).RouteAsync(
            Arg.Is<ClassifiedIntent>(i =>
                i.ActionType == ActionType.CreateProperty &&
                i.Confidence == 1.0 &&
                i.RawFunctionName == "create_property" &&
                i.Parameters.ContainsKey("title")),
            TenantId,
            AgentId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleToolCallAsync_WithUnknownTool_ShouldReturnFailure()
    {
        // Arrange
        var arguments = new Dictionary<string, object?>();

        // Act
        var result = await _handler.HandleToolCallAsync(
            "nonexistent_tool", arguments, TenantId, AgentId, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.Unknown);
        result.Errors.Should().NotBeEmpty();

        await _router.DidNotReceive().RouteAsync(
            Arg.Any<ClassifiedIntent>(),
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleToolCallAsync_WithJsonElementArguments_ShouldForwardToRouter()
    {
        // Arrange — simulate what MCP SDK sends (JsonElement values)
        var jsonDoc = JsonDocument.Parse("""{"title":"A flat","price":250000}""");
        var arguments = new Dictionary<string, object?>();
        foreach (var prop in jsonDoc.RootElement.EnumerateObject())
        {
            arguments[prop.Name] = prop.Value;
        }

        var expectedResult = ActionResult.Ok(
            data: null,
            message: "Property created.",
            type: ActionType.CreateProperty);

        _router.RouteAsync(
            Arg.Any<ClassifiedIntent>(),
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.HandleToolCallAsync(
            "create_property", arguments, TenantId, AgentId, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        await _router.Received(1).RouteAsync(
            Arg.Is<ClassifiedIntent>(i =>
                i.Parameters.Count == 2),
            TenantId,
            AgentId,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("create_property", ActionType.CreateProperty)]
    [InlineData("update_property", ActionType.UpdateProperty)]
    [InlineData("query_properties", ActionType.QueryProperties)]
    [InlineData("change_property_status", ActionType.ChangePropertyStatus)]
    [InlineData("generate_copy", ActionType.GenerateCopy)]
    [InlineData("extract_from_text", ActionType.ExtractFromText)]
    [InlineData("extract_from_photos", ActionType.ExtractFromPhotos)]
    [InlineData("create_lead", ActionType.CreateLead)]
    [InlineData("create_contact", ActionType.CreateContact)]
    [InlineData("qualify_lead", ActionType.QualifyLead)]
    [InlineData("convert_lead", ActionType.ConvertLead)]
    [InlineData("query_leads", ActionType.QueryLeads)]
    [InlineData("book_viewing", ActionType.BookViewing)]
    [InlineData("query_appointments", ActionType.QueryAppointments)]
    [InlineData("cancel_appointment", ActionType.CancelAppointment)]
    [InlineData("reschedule_appointment", ActionType.RescheduleAppointment)]
    [InlineData("reserve_property", ActionType.ReserveProperty)]
    [InlineData("close_operation", ActionType.CloseOperation)]
    [InlineData("archive_property", ActionType.ArchiveProperty)]
    [InlineData("reactivate_property", ActionType.ReactivateProperty)]
    public async Task HandleToolCallAsync_ResolvesCorrectActionType(string toolName, ActionType expectedType)
    {
        // Arrange
        var expectedResult = ActionResult.Ok(data: null, message: "ok", type: expectedType);
        _router.RouteAsync(
            Arg.Any<ClassifiedIntent>(),
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.HandleToolCallAsync(
            toolName, new Dictionary<string, object?>(), TenantId, AgentId, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(expectedType);

        await _router.Received(1).RouteAsync(
            Arg.Is<ClassifiedIntent>(i => i.ActionType == expectedType),
            TenantId,
            AgentId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleToolCallAsync_WhenRouterThrows_ShouldReturnFailure()
    {
        // Arrange
        _router.RouteAsync(
            Arg.Any<ClassifiedIntent>(),
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>())
            .Returns<ActionResult>(_ => throw new InvalidOperationException("Something broke"));

        // Act
        var result = await _handler.HandleToolCallAsync(
            "create_property",
            new Dictionary<string, object?>(),
            TenantId,
            AgentId,
            CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.CreateProperty);
        result.Errors.Should().Contain(e => e.Contains("error"));
    }

    [Fact]
    public async Task HandleToolCallAsync_WithNullArguments_ShouldPassEmptyDict()
    {
        // Arrange
        var expectedResult = ActionResult.Ok(data: null, message: "ok", type: ActionType.CreateProperty);
        _router.RouteAsync(
            Arg.Any<ClassifiedIntent>(),
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.HandleToolCallAsync(
            "create_property", null, TenantId, AgentId, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        await _router.Received(1).RouteAsync(
            Arg.Is<ClassifiedIntent>(i => i.Parameters.Count == 0),
            TenantId,
            AgentId,
            Arg.Any<CancellationToken>());
    }
}
