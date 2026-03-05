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

public sealed class ExtractFromPhotosActionHandlerTests
{
    private readonly IOpenAiService _openAiService;
    private readonly ExtractFromPhotosActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public ExtractFromPhotosActionHandlerTests()
    {
        _openAiService = Substitute.For<IOpenAiService>();
        var logger = Substitute.For<ILogger<ExtractFromPhotosActionHandler>>();
        _handler = new ExtractFromPhotosActionHandler(_openAiService, logger);
    }

    [Fact]
    public async Task Handle_WhenValidImageUrls_ShouldCallOpenAiAndReturnExtractedData()
    {
        // Arrange
        var imageUrls = new List<string>
        {
            "https://example.com/photo1.jpg",
            "https://example.com/photo2.jpg"
        };
        var parameters = new Dictionary<string, object?> { ["image_urls"] = imageUrls };
        var command = new ExtractFromPhotosActionCommand(parameters, TenantId, AgentId);

        var aiResponse = """
        {
            "property_type": { "value": "apartment", "confidence": 0.85 },
            "bedrooms": { "value": 2, "confidence": 0.8 },
            "features": { "value": ["hardwood floors", "modern kitchen"], "confidence": 0.9 },
            "description": { "value": "Modern apartment with open-plan layout", "confidence": 0.85 }
        }
        """;

        _openAiService.AnalyzeImagesAsync(
            Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<CancellationToken>())
            .Returns(aiResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.ExtractFromPhotos);
        result.Data.Should().NotBeNull();
        result.Data.Should().BeOfType<ExtractedPropertyDto>();

        var dto = (ExtractedPropertyDto)result.Data!;
        dto.PropertyType!.Value.Should().Be("apartment");
        dto.Bedrooms!.Value.Should().Be(2);
        dto.Features!.Value.Should().Contain("hardwood floors");
    }

    [Fact]
    public async Task Handle_WhenMissingImageUrls_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>();
        var command = new ExtractFromPhotosActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ExtractFromPhotos);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("image");
    }

    [Fact]
    public async Task Handle_WhenEmptyImageUrls_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new Dictionary<string, object?> { ["image_urls"] = new List<string>() };
        var command = new ExtractFromPhotosActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ExtractFromPhotos);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenMoreThan10Images_ShouldReturnFailure()
    {
        // Arrange
        var imageUrls = Enumerable.Range(1, 11)
            .Select(i => $"https://example.com/photo{i}.jpg")
            .ToList();
        var parameters = new Dictionary<string, object?> { ["image_urls"] = imageUrls };
        var command = new ExtractFromPhotosActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ExtractFromPhotos);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("10");
    }

    [Fact]
    public async Task Handle_WhenExactly10Images_ShouldSucceed()
    {
        // Arrange
        var imageUrls = Enumerable.Range(1, 10)
            .Select(i => $"https://example.com/photo{i}.jpg")
            .ToList();
        var parameters = new Dictionary<string, object?> { ["image_urls"] = imageUrls };
        var command = new ExtractFromPhotosActionCommand(parameters, TenantId, AgentId);

        var aiResponse = """{ "property_type": { "value": "house", "confidence": 0.8 } }""";

        _openAiService.AnalyzeImagesAsync(
            Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<CancellationToken>())
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
        var imageUrls = new List<string> { "https://example.com/photo1.jpg" };
        var parameters = new Dictionary<string, object?> { ["image_urls"] = imageUrls };
        var command = new ExtractFromPhotosActionCommand(parameters, TenantId, AgentId);

        _openAiService.AnalyzeImagesAsync(
            Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<CancellationToken>())
            .Returns(string.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ExtractFromPhotos);
    }

    [Fact]
    public async Task Handle_WhenOpenAiThrows_ShouldReturnFailure()
    {
        // Arrange
        var imageUrls = new List<string> { "https://example.com/photo1.jpg" };
        var parameters = new Dictionary<string, object?> { ["image_urls"] = imageUrls };
        var command = new ExtractFromPhotosActionCommand(parameters, TenantId, AgentId);

        _openAiService.AnalyzeImagesAsync(
            Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Vision API error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.ExtractFromPhotos);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectImageUrlsToOpenAi()
    {
        // Arrange
        var imageUrls = new List<string>
        {
            "https://example.com/photo1.jpg",
            "https://example.com/photo2.jpg",
            "https://example.com/photo3.jpg"
        };
        var parameters = new Dictionary<string, object?> { ["image_urls"] = imageUrls };
        var command = new ExtractFromPhotosActionCommand(parameters, TenantId, AgentId);

        var aiResponse = """{ "property_type": { "value": "villa", "confidence": 0.9 } }""";

        _openAiService.AnalyzeImagesAsync(
            Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<CancellationToken>())
            .Returns(aiResponse);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _openAiService.Received(1).AnalyzeImagesAsync(
            Arg.Is<string>(s => s.Contains("photo analysis")),
            Arg.Is<string[]>(urls => urls.Length == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSingleImage_ShouldSucceed()
    {
        // Arrange
        var imageUrls = new List<string> { "https://example.com/photo1.jpg" };
        var parameters = new Dictionary<string, object?> { ["image_urls"] = imageUrls };
        var command = new ExtractFromPhotosActionCommand(parameters, TenantId, AgentId);

        var aiResponse = """
        {
            "property_type": { "value": "studio", "confidence": 0.7 },
            "has_pool": { "value": false, "confidence": 0.8 }
        }
        """;

        _openAiService.AnalyzeImagesAsync(
            Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<CancellationToken>())
            .Returns(aiResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var dto = (ExtractedPropertyDto)result.Data!;
        dto.PropertyType!.Value.Should().Be("studio");
        dto.HasPool!.Value.Should().Be(false);
    }
}
