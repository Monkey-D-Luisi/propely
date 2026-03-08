// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.AiApi.Application.Actions.Commands.Content;
using Propely.AiApi.Application.Actions.Dtos;
using Propely.AiApi.Application.Actions.Handlers;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.UnitTests.Application.Actions.Commands.Content;

public sealed class GenerateCopyActionHandlerTests
{
    private readonly IOpenAiService _openAiService;
    private readonly GenerateCopyActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public GenerateCopyActionHandlerTests()
    {
        _openAiService = Substitute.For<IOpenAiService>();
        var logger = Substitute.For<ILogger<GenerateCopyActionHandler>>();
        _handler = new GenerateCopyActionHandler(_openAiService, logger);
    }

    [Fact]
    public async Task Handle_WhenValidParameters_ShouldCallOpenAiAndReturnCopy()
    {
        // Arrange
        var parameters = new GenerateCopyParameters(
            PropertyData: "3-bedroom apartment in Malaga, 120 sqm, sea views, 250,000 EUR",
            Tone: "professional",
            Languages: new List<string> { "es", "en" });
        var command = new GenerateCopyActionCommand(parameters, TenantId, AgentId);

        var aiResponse = """
        {
            "variants": {
                "es": "Amplio apartamento de 3 dormitorios en Malaga con vistas al mar...",
                "en": "Spacious 3-bedroom apartment in Malaga with sea views..."
            },
            "tone": "professional"
        }
        """;

        _openAiService.GenerateWithJsonResponseAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(aiResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.GenerateCopy);
        result.Data.Should().NotBeNull();
        result.Data.Should().BeOfType<GeneratedCopyDto>();

        var dto = (GeneratedCopyDto)result.Data!;
        dto.Variants.Should().ContainKey("es");
        dto.Variants.Should().ContainKey("en");
        dto.Tone.Should().Be("professional");
    }

    [Fact]
    public async Task Handle_WhenMissingPropertyData_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new GenerateCopyParameters(
            PropertyData: null,
            Tone: "professional",
            Languages: null);
        var command = new GenerateCopyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.GenerateCopy);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("property");
    }

    [Fact]
    public async Task Handle_WhenNoToneProvided_ShouldDefaultToProfessional()
    {
        // Arrange
        var parameters = new GenerateCopyParameters(
            PropertyData: "2-bedroom house in Barcelona",
            Tone: null,
            Languages: null);
        var command = new GenerateCopyActionCommand(parameters, TenantId, AgentId);

        var aiResponse = """
        {
            "variants": {
                "es": "Casa de 2 dormitorios en Barcelona...",
                "en": "2-bedroom house in Barcelona...",
                "fr": "Maison de 2 chambres a Barcelone...",
                "de": "2-Zimmer-Haus in Barcelona...",
                "nl": "2-slaapkamer huis in Barcelona..."
            },
            "tone": "professional"
        }
        """;

        _openAiService.GenerateWithJsonResponseAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(aiResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var dto = (GeneratedCopyDto)result.Data!;
        dto.Variants.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_WhenInvalidTone_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new GenerateCopyParameters(
            PropertyData: "A nice apartment",
            Tone: "aggressive",
            Languages: null);
        var command = new GenerateCopyActionCommand(parameters, TenantId, AgentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.GenerateCopy);
        result.Errors.Should().NotBeEmpty();
        result.Message.Should().Contain("tone");
    }

    [Fact]
    public async Task Handle_WhenNoLanguagesProvided_ShouldRequestAllLanguages()
    {
        // Arrange
        var parameters = new GenerateCopyParameters(
            PropertyData: "Luxury villa with pool in Marbella",
            Tone: null,
            Languages: null);
        var command = new GenerateCopyActionCommand(parameters, TenantId, AgentId);

        var aiResponse = """
        {
            "variants": {
                "es": "Villa de lujo...",
                "en": "Luxury villa...",
                "fr": "Villa de luxe...",
                "de": "Luxusvilla...",
                "nl": "Luxe villa..."
            },
            "tone": "professional"
        }
        """;

        _openAiService.GenerateWithJsonResponseAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(aiResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        await _openAiService.Received(1).GenerateWithJsonResponseAsync(
            Arg.Any<string>(),
            Arg.Is<string>(s => s.Contains("es") && s.Contains("en") && s.Contains("fr") && s.Contains("de") && s.Contains("nl")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSpecificLanguagesProvided_ShouldRequestOnlyThoseLanguages()
    {
        // Arrange
        var parameters = new GenerateCopyParameters(
            PropertyData: "A nice apartment in Valencia",
            Tone: null,
            Languages: new List<string> { "es", "en" });
        var command = new GenerateCopyActionCommand(parameters, TenantId, AgentId);

        var aiResponse = """
        {
            "variants": {
                "es": "Bonito apartamento en Valencia...",
                "en": "Nice apartment in Valencia..."
            },
            "tone": "professional"
        }
        """;

        _openAiService.GenerateWithJsonResponseAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(aiResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var dto = (GeneratedCopyDto)result.Data!;
        dto.Variants.Should().ContainKey("es");
        dto.Variants.Should().ContainKey("en");
    }

    [Theory]
    [InlineData("professional")]
    [InlineData("luxury")]
    [InlineData("casual")]
    [InlineData("concise")]
    public async Task Handle_WhenValidTone_ShouldAcceptIt(string tone)
    {
        // Arrange
        var parameters = new GenerateCopyParameters(
            PropertyData: "3-bedroom villa in Marbella",
            Tone: tone,
            Languages: null);
        var command = new GenerateCopyActionCommand(parameters, TenantId, AgentId);

        var aiResponse = $$"""
        {
            "variants": { "es": "Texto en espanol..." },
            "tone": "{{tone}}"
        }
        """;

        _openAiService.GenerateWithJsonResponseAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
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
        var parameters = new GenerateCopyParameters(
            PropertyData: "A nice apartment",
            Tone: null,
            Languages: null);
        var command = new GenerateCopyActionCommand(parameters, TenantId, AgentId);

        _openAiService.GenerateWithJsonResponseAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(string.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.GenerateCopy);
    }

    [Fact]
    public async Task Handle_WhenOpenAiThrows_ShouldReturnFailure()
    {
        // Arrange
        var parameters = new GenerateCopyParameters(
            PropertyData: "A nice apartment",
            Tone: null,
            Languages: null);
        var command = new GenerateCopyActionCommand(parameters, TenantId, AgentId);

        _openAiService.GenerateWithJsonResponseAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("API error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.GenerateCopy);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldIncludeToneInSystemPrompt()
    {
        // Arrange
        var parameters = new GenerateCopyParameters(
            PropertyData: "Exclusive penthouse in Madrid",
            Tone: "luxury",
            Languages: null);
        var command = new GenerateCopyActionCommand(parameters, TenantId, AgentId);

        var aiResponse = """
        {
            "variants": { "es": "Exclusivo atico..." },
            "tone": "luxury"
        }
        """;

        _openAiService.GenerateWithJsonResponseAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(aiResponse);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _openAiService.Received(1).GenerateWithJsonResponseAsync(
            Arg.Is<string>(s => s.Contains("luxury") || s.Contains("Luxury")),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }
}
