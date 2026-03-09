// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Propely.AiApi.Infrastructure.Services;

namespace Propely.AiApi.UnitTests.Infrastructure.Services;

public sealed class OpenAiVoiceTranscriptionServiceTests
{
    private readonly ILogger<OpenAiVoiceTranscriptionService> _logger;

    public OpenAiVoiceTranscriptionServiceTests()
    {
        _logger = Substitute.For<ILogger<OpenAiVoiceTranscriptionService>>();
    }

    [Fact]
    public void Constructor_WhenApiKeyIsNull_ShouldNotThrow()
    {
        // Arrange
        var options = Options.Create(new OpenAiOptions { ApiKey = null });

        // Act
        var act = () => new OpenAiVoiceTranscriptionService(options, _logger);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WhenApiKeyIsEmpty_ShouldNotThrow()
    {
        // Arrange
        var options = Options.Create(new OpenAiOptions { ApiKey = "" });

        // Act
        var act = () => new OpenAiVoiceTranscriptionService(options, _logger);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WhenApiKeyIsWhitespace_ShouldNotThrow()
    {
        // Arrange
        var options = Options.Create(new OpenAiOptions { ApiKey = "   " });

        // Act
        var act = () => new OpenAiVoiceTranscriptionService(options, _logger);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task TranscribeAsync_WhenApiKeyNotConfigured_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new OpenAiOptions { ApiKey = null });
        var service = new OpenAiVoiceTranscriptionService(options, _logger);
        using var stream = new MemoryStream([1, 2, 3]);

        // Act
        var act = async () => await service.TranscribeAsync(stream, "test.wav", null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*API key*not configured*");
    }

    [Fact]
    public async Task TranscribeAsync_WhenApiKeyIsEmpty_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new OpenAiOptions { ApiKey = "" });
        var service = new OpenAiVoiceTranscriptionService(options, _logger);
        using var stream = new MemoryStream([1, 2, 3]);

        // Act
        var act = async () => await service.TranscribeAsync(stream, "test.mp3", "es", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*API key*not configured*");
    }

    [Fact]
    public void Constructor_WhenApiKeyIsProvided_ShouldCreateClientWithTranscriptionModel()
    {
        // Arrange
        var options = Options.Create(new OpenAiOptions
        {
            ApiKey = "sk-test-key-12345",
            TranscriptionModelId = "gpt-4o-mini-transcribe"
        });

        // Act — should not throw; AudioClient will be created
        var service = new OpenAiVoiceTranscriptionService(options, _logger);

        // Assert — service was created successfully (client is private, but no exception means success)
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WhenCustomTranscriptionModelProvided_ShouldNotThrow()
    {
        // Arrange
        var options = Options.Create(new OpenAiOptions
        {
            ApiKey = "sk-test-key-12345",
            TranscriptionModelId = "gpt-4o-mini-transcribe"
        });

        // Act
        var act = () => new OpenAiVoiceTranscriptionService(options, _logger);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void OpenAiOptions_DefaultTranscriptionModelId_ShouldBeGpt4oMiniTranscribe()
    {
        // Arrange & Act
        var options = new OpenAiOptions();

        // Assert
        options.TranscriptionModelId.Should().Be("gpt-4o-mini-transcribe");
    }

    [Fact]
    public void OpenAiOptions_TranscriptionModelId_ShouldBeSettable()
    {
        // Arrange
        var options = new OpenAiOptions();

        // Act
        options.TranscriptionModelId = "whisper-1";

        // Assert
        options.TranscriptionModelId.Should().Be("whisper-1");
    }
}
