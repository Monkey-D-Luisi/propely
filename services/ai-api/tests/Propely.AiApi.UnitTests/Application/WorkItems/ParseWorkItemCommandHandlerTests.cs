// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Application.WorkItems.Commands.ParseWorkItem;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Propely.AiApi.UnitTests.Application.WorkItems;

public sealed class ParseWorkItemCommandHandlerTests
{
    private readonly IOpenAiService _openAiService;
    private readonly ParseWorkItemCommandHandler _handler;

    public ParseWorkItemCommandHandlerTests()
    {
        _openAiService = Substitute.For<IOpenAiService>();
        var logger = Substitute.For<ILogger<ParseWorkItemCommandHandler>>();
        _handler = new ParseWorkItemCommandHandler(_openAiService, logger);
    }

    [Fact]
    public async Task Handle_WithValidAiResponse_ShouldReturnParsedFields()
    {
        // Arrange
        var command = new ParseWorkItemCommand("Bug in login that doesn't validate emails with +, set to active");
        _openAiService.GenerateTextAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("""{"title":"Login email validation bug","description":"Login does not validate emails containing the + character","status":"Active","priority":"High","type":"Bug","dueDate":"2026-03-01","estimatedEffort":"L","confidence":0.9}""");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Title.Should().Be("Login email validation bug");
        result.Description.Should().Be("Login does not validate emails containing the + character");
        result.Status.Should().Be("Active");
        result.Priority.Should().Be("High");
        result.Type.Should().Be("Bug");
        result.DueDateUtc.Should().Be("2026-03-01");
        result.EstimatedEffort.Should().Be("L");
        result.Confidence.Should().BeApproximately(0.9, 0.01);
    }

    [Fact]
    public async Task Handle_WithMarkdownFencedResponse_ShouldStripFencesAndParse()
    {
        // Arrange
        var command = new ParseWorkItemCommand("Fix the homepage layout");
        _openAiService.GenerateTextAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("```json\n{\"title\":\"Fix homepage layout\",\"description\":null,\"status\":\"Pending\",\"priority\":\"Medium\",\"type\":\"Task\",\"dueDate\":null,\"estimatedEffort\":\"M\",\"confidence\":0.85}\n```");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Title.Should().Be("Fix homepage layout");
        result.Description.Should().BeNull();
        result.Status.Should().Be("Pending");
        result.Priority.Should().Be("Medium");
        result.Type.Should().Be("Task");
        result.DueDateUtc.Should().BeNull();
        result.EstimatedEffort.Should().Be("M");
    }

    [Fact]
    public async Task Handle_WithInvalidStatus_ShouldDefaultToPending()
    {
        // Arrange
        var command = new ParseWorkItemCommand("Some task");
        _openAiService.GenerateTextAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("""{"title":"Some task","description":null,"status":"InProgress","priority":"Medium","type":"Task","dueDate":null,"estimatedEffort":"M","confidence":0.7}""");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Pending");
        result.Priority.Should().Be("Medium");
        result.Type.Should().Be("Task");
        result.EstimatedEffort.Should().Be("M");
    }

    [Fact]
    public async Task Handle_WhenAiServiceThrows_ShouldFallbackToRawText()
    {
        // Arrange
        var command = new ParseWorkItemCommand("Fix the bug");
        _openAiService.GenerateTextAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("OpenAI API key is not configured."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Title.Should().Be("Fix the bug");
        result.Description.Should().BeNull();
        result.Status.Should().Be("Pending");
        result.Priority.Should().Be("Medium");
        result.Type.Should().Be("Task");
        result.DueDateUtc.Should().BeNull();
        result.EstimatedEffort.Should().Be("M");
        result.Confidence.Should().Be(0.0);
    }

    [Fact]
    public async Task Handle_WithInvalidJson_ShouldFallbackToRawText()
    {
        // Arrange
        var command = new ParseWorkItemCommand("Create a new feature");
        _openAiService.GenerateTextAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("This is not JSON at all");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Title.Should().Be("Create a new feature");
        result.Priority.Should().Be("Medium");
        result.Type.Should().Be("Task");
        result.DueDateUtc.Should().BeNull();
        result.EstimatedEffort.Should().Be("M");
        result.Confidence.Should().Be(0.0);
    }

    [Fact]
    public async Task Handle_WithLongText_ShouldTruncateTitleTo200Chars()
    {
        // Arrange
        var longText = new string('a', 300);
        var command = new ParseWorkItemCommand(longText);
        _openAiService.GenerateTextAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("AI unavailable"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Title.Should().HaveLength(200);
    }
}
