// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.AiApi.Application.Actions.Commands.Content;
using Propely.AiApi.Application.Actions.Dtos;
using Propely.AiApi.Application.Actions.Handlers;
using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.UnitTests.Application.Actions.Commands.Content;

public sealed class ExtractFromTextActionHandlerTests
{
    private readonly IOpenAiService _openAiService;
    private readonly ExtractFromTextActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public ExtractFromTextActionHandlerTests()
    {
        _openAiService = Substitute.For<IOpenAiService>();
        var logger = Substitute.For<ILogger<ExtractFromTextActionHandler>>();
        _handler = new ExtractFromTextActionHandler(_openAiService, logger);
    }

    [Fact]
    public async Task Handle_WhenValidText_ShouldCallOpenAiAndReturnExtractedData()
    {
        // Arrange
        var inputText = "Beautiful 3-bedroom apartment in Malaga for sale at 250,000 EUR with 2 bathrooms and a terrace.";
        var parameters = new Dictionary<string, object?> { ["text"] = inputText };
        var command = new ExtractFromTextActionCommand(parameters, TenantId, AgentId);

        var aiResponse = """
        {
            "property_type": { "value": "apartment", "confidence": 0.95 },
            "operation_type": { "value": "sale", "confidence": 0.9 },
            "bedrooms": { "value": 3, "confidence": 0.95 },
            "bathrooms": { "value": 2, "confidence": 0.9 },
            "price": { "value": 250000.00, "confidence": 0.9 },
            "city": { "value": "Malaga", "confidence": 0.95 },
            "features": { "value": ["terrace"], "confidence": 0.85 }
        }
        """;

        _openAiService.GenerateWithJsonResponseAsync(
            Arg.Any<string>(), Arg.Is(inputText), Arg.Any<CancellationToken>())
            .Returns(aiResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.ExtractFromText);
        result.Data.Should().NotBeNull();
        result.Data.Should().BeOfType<ExtractedPropertyDto>();

        var dto = (ExtractedPropertyDto)result.Data!;
        dto.PropertyType!.Value.Should().Be("apartment");
        dto.PropertyType.Confidence.Should().Be(0.95);
        dto.Bedrooms!.Value.Should().Be(3);
        dto.City!.Value.Should().Be("Malaga");
        dto.Features!.Value.Should().Contain("terrace");
    }

    [Fact]
    public async Task Handle_WhenMissingText_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>();
        var command = new ExtractFromTextActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ExtractFromText);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("text");
    }

    [Fact]
    public async Task Handle_WhenEmptyText_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new Dictionary<string, object?> { ["text"] = "   " };
        var command = new ExtractFromTextActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ExtractFromText);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenTextExceedsMaxLength_ShouldReturnFailure()
    {
        // Arrange
        var longText = new string('a', 10001);
        var parameters = new Dictionary<string, object?> { ["text"] = longText };
        var command = new ExtractFromTextActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ExtractFromText);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("10,000");
    }

    [Fact]
    public async Task Handle_WhenTextAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var maxText = new string('a', 10000);
        var parameters = new Dictionary<string, object?> { ["text"] = maxText };
        var command = new ExtractFromTextActionCommand(parameters, TenantId, AgentId);

        var aiResponse = """{ "property_type": { "value": "apartment", "confidence": 0.5 } }""";

        _openAiService.GenerateWithJsonResponseAsync(
            Arg.Any<string>(), Arg.Is(maxText), Arg.Any<CancellationToken>())
            .Returns(aiResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenOpenAiReturnsEmptyResponse_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new Dictionary<string, object?> { ["text"] = "Some property text" };
        var command = new ExtractFromTextActionCommand(parameters, TenantId, AgentId);

        _openAiService.GenerateWithJsonResponseAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(string.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ExtractFromText);
    }

    [Fact]
    public async Task Handle_WhenOpenAiThrows_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new Dictionary<string, object?> { ["text"] = "Some property text" };
        var command = new ExtractFromTextActionCommand(parameters, TenantId, AgentId);

        _openAiService.GenerateWithJsonResponseAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("API error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ExtractFromText);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPartialResponse_ShouldReturnAvailableFields()
    {
        // Arrange
        var parameters = new Dictionary<string, object?> { ["text"] = "A villa in Barcelona" };
        var command = new ExtractFromTextActionCommand(parameters, TenantId, AgentId);

        var aiResponse = """
        {
            "property_type": { "value": "villa", "confidence": 0.9 },
            "city": { "value": "Barcelona", "confidence": 0.95 }
        }
        """;

        _openAiService.GenerateWithJsonResponseAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(aiResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var dto = (ExtractedPropertyDto)result.Data!;
        dto.PropertyType!.Value.Should().Be("villa");
        dto.City!.Value.Should().Be("Barcelona");
        dto.Bedrooms.Should().BeNull();
        dto.Price.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallOpenAiWithCorrectPromptStructure()
    {
        // Arrange
        var inputText = "A small studio apartment in Madrid";
        var parameters = new Dictionary<string, object?> { ["text"] = inputText };
        var command = new ExtractFromTextActionCommand(parameters, TenantId, AgentId);

        var aiResponse = """{ "property_type": { "value": "studio", "confidence": 0.9 } }""";

        _openAiService.GenerateWithJsonResponseAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(aiResponse);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _openAiService.Received(1).GenerateWithJsonResponseAsync(
            Arg.Is<string>(s => s.Contains("real estate") && s.Contains("confidence")),
            Arg.Is(inputText),
            Arg.Any<CancellationToken>());
    }
}
