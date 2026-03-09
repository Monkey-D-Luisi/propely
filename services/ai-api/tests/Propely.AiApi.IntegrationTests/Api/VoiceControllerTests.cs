// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.IntegrationTests.Fixtures;

namespace Propely.AiApi.IntegrationTests.Api;

/// <summary>
/// Integration tests for VoiceController endpoints.
/// Uses a mocked IVoiceTranscriptionService to avoid calling OpenAI in tests.
/// Tests the full HTTP pipeline: auth → validation → transcription → action execution → response.
/// </summary>
public sealed class VoiceControllerTests : IClassFixture<VoiceApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly IVoiceTranscriptionService _mockTranscription;

    public VoiceControllerTests(VoiceApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _mockTranscription = factory.MockTranscriptionService;
    }

    [Fact]
    public async Task Execute_ReturnsForbid_WhenNoOrgClaim()
    {
        // Arrange: authenticated user but no org claim
        var content = CreateAudioMultipartContent();
        var request = new HttpRequestMessage(HttpMethod.Post, "/v1/voice/execute")
        {
            Content = content
        };
        request.Headers.Add("X-Test-Org-Id", "none");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenNoAudioFile()
    {
        // Arrange: empty multipart form (no audio field)
        var content = new MultipartFormDataContent();

        // Act
        var response = await _client.PostAsync("/v1/voice/execute", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenUnsupportedContentType()
    {
        // Arrange: wrong MIME type
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x01, 0x02 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "audio", "recording.txt");

        // Act
        var response = await _client.PostAsync("/v1/voice/execute", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Execute_ReturnsOk_WithTranscriptionAndAction()
    {
        // Arrange: mock transcription returns Spanish text
        _mockTranscription.TranscribeAsync(
            Arg.Any<Stream>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>()
        ).Returns(new TranscriptionResult(
            Text: "Crear una propiedad en Sabadell",
            Language: "es",
            DurationMs: 3200));

        var content = CreateAudioMultipartContent();

        // Act
        var response = await _client.PostAsync("/v1/voice/execute", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("transcribedText").GetString().Should().Be("Crear una propiedad en Sabadell");
        root.GetProperty("language").GetString().Should().Be("es");
        root.GetProperty("durationMs").GetInt32().Should().Be(3200);
        root.TryGetProperty("action", out var action).Should().BeTrue();
        action.GetProperty("actionType").GetString().Should().NotBeNullOrEmpty();
        action.GetProperty("message").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Execute_ReturnsEmptyTranscription_WhenNoSpeechDetected()
    {
        // Arrange: mock transcription returns empty text (silent audio)
        _mockTranscription.TranscribeAsync(
            Arg.Any<Stream>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>()
        ).Returns(new TranscriptionResult(
            Text: "",
            Language: "unknown",
            DurationMs: 1000));

        var content = CreateAudioMultipartContent();

        // Act
        var response = await _client.PostAsync("/v1/voice/execute", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("transcribedText").GetString().Should().BeEmpty();
        root.GetProperty("action").GetProperty("success").GetBoolean().Should().BeFalse();
        root.GetProperty("action").GetProperty("message").GetString().Should().Contain("No speech detected");
    }

    [Fact]
    public async Task Execute_ForwardsLanguageHint()
    {
        // Arrange
        _mockTranscription.TranscribeAsync(
            Arg.Any<Stream>(),
            Arg.Any<string>(),
            Arg.Is<string?>(l => l == "es"),
            Arg.Any<CancellationToken>()
        ).Returns(new TranscriptionResult(
            Text: "Listar propiedades",
            Language: "es",
            DurationMs: 1500));

        var content = CreateAudioMultipartContent();
        content.Add(new StringContent("es"), "language");

        // Act
        var response = await _client.PostAsync("/v1/voice/execute", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await _mockTranscription.Received(1).TranscribeAsync(
            Arg.Any<Stream>(),
            Arg.Any<string>(),
            "es",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Transcribe_ReturnsOk_WithTranscriptionResult()
    {
        // Arrange
        _mockTranscription.TranscribeAsync(
            Arg.Any<Stream>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>()
        ).Returns(new TranscriptionResult(
            Text: "Hello world",
            Language: "en",
            DurationMs: 2000));

        var content = CreateAudioMultipartContent();

        // Act
        var response = await _client.PostAsync("/v1/voice/transcribe", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("text").GetString().Should().Be("Hello world");
        root.GetProperty("language").GetString().Should().Be("en");
        root.GetProperty("durationMs").GetInt32().Should().Be(2000);
    }

    /// <summary>
    /// Creates a minimal valid multipart/form-data content with a fake WebM audio file.
    /// The actual audio content doesn't matter because transcription is mocked.
    /// </summary>
    private static MultipartFormDataContent CreateAudioMultipartContent()
    {
        var content = new MultipartFormDataContent();
        // Create a minimal WebM-like file (just needs valid content type + extension)
        var audioBytes = CreateMinimalWavBytes();
        var fileContent = new ByteArrayContent(audioBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/webm");
        content.Add(fileContent, "audio", "recording.webm");
        return content;
    }

    /// <summary>
    /// Generates a minimal valid WAV file (44-byte header + 1 second of random noise at 8kHz mono 8-bit).
    /// Random data ensures each call produces a unique SHA256 hash, avoiding the server-side
    /// deduplication guard that rejects identical audio within a 30-second window.
    /// </summary>
    private static byte[] CreateMinimalWavBytes()
    {
        const int sampleRate = 8000;
        const int numSamples = 8000; // 1 second
        const int bitsPerSample = 8;
        const int numChannels = 1;

        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        var dataSize = numSamples * numChannels * (bitsPerSample / 8);
        var fileSize = 36 + dataSize;

        // RIFF header
        writer.Write("RIFF"u8);
        writer.Write(fileSize);
        writer.Write("WAVE"u8);

        // fmt chunk
        writer.Write("fmt "u8);
        writer.Write(16); // chunk size
        writer.Write((short)1); // PCM
        writer.Write((short)numChannels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * numChannels * (bitsPerSample / 8)); // byte rate
        writer.Write((short)(numChannels * (bitsPerSample / 8))); // block align
        writer.Write((short)bitsPerSample);

        // data chunk — random bytes to produce unique hashes per test
        var audioData = new byte[dataSize];
        Random.Shared.NextBytes(audioData);
        writer.Write("data"u8);
        writer.Write(dataSize);
        writer.Write(audioData);

        return ms.ToArray();
    }
}
