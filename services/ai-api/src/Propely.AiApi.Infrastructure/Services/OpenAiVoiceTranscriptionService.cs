// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Audio;
using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Domain.Common.Exceptions;

namespace Propely.AiApi.Infrastructure.Services;

/// <summary>
/// Voice transcription service using OpenAI's gpt-4o-mini-transcription model.
/// </summary>
public class OpenAiVoiceTranscriptionService : IVoiceTranscriptionService
{
    private readonly AudioClient? _audioClient;
    private readonly ILogger<OpenAiVoiceTranscriptionService> _logger;

    public OpenAiVoiceTranscriptionService(
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiVoiceTranscriptionService> logger)
    {
        _logger = logger;

        var apiKey = options.Value.ApiKey;
        var modelId = options.Value.TranscriptionModelId;

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _audioClient = new AudioClient(modelId, apiKey);
        }
    }

    /// <summary>
    /// Internal constructor for testing — allows injecting a pre-built AudioClient.
    /// </summary>
    internal OpenAiVoiceTranscriptionService(
        AudioClient audioClient,
        ILogger<OpenAiVoiceTranscriptionService> logger)
    {
        _audioClient = audioClient;
        _logger = logger;
    }

    public async Task<TranscriptionResult> TranscribeAsync(
        Stream audio,
        string fileName,
        string? languageHint,
        CancellationToken ct)
    {
        if (_audioClient == null)
        {
            throw new InvalidOperationException(
                "OpenAI API key is not configured. Set the AIAPI_OpenAi__ApiKey environment variable.");
        }

        try
        {
            var options = new AudioTranscriptionOptions();

            if (!string.IsNullOrWhiteSpace(languageHint))
            {
                options.Language = languageHint;
            }

            options.ResponseFormat = AudioTranscriptionFormat.Simple;

            _logger.LogInformation("Transcribing audio file {FileName} with language hint {Language}",
                fileName, languageHint ?? "auto");

            var transcription = await _audioClient.TranscribeAudioAsync(audio, fileName, options, ct);

            var text = transcription.Value.Text ?? string.Empty;
            var language = transcription.Value.Language ?? languageHint ?? "unknown";
            var durationMs = transcription.Value.Duration.HasValue
                ? (int)transcription.Value.Duration.Value.TotalMilliseconds
                : 0;

            _logger.LogInformation("Transcription completed: {CharCount} chars, language={Language}, duration={DurationMs}ms",
                text.Length, language, durationMs);

            return new TranscriptionResult(text, language, durationMs);
        }
        catch (Exception ex) when (ex is not AiServiceException and not InvalidOperationException)
        {
            _logger.LogError(ex, "Failed to transcribe audio file {FileName}", fileName);
            throw new AiServiceException("Failed to transcribe audio via OpenAI.", ex);
        }
    }
}
