// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Client.Dtos;
using Refit;

namespace Propely.AiApi.Client;

/// <summary>
/// Refit-based typed HTTP client for the AI API voice endpoints.
/// Handles audio file transcription and voice-driven action execution.
/// </summary>
public interface IVoiceApi
{
    /// <summary>
    /// Transcribes an audio file to text using OpenAI speech-to-text.
    /// </summary>
    /// <param name="audio">The audio file to transcribe (WebM, WAV, MP3, M4A, OGG, FLAC).</param>
    /// <param name="language">Optional BCP-47 language hint (e.g., "es", "en").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The transcription result with text, language, and duration.</returns>
    [Multipart]
    [Post("/v1/voice/transcribe")]
    Task<TranscriptionResultDto> TranscribeAsync(
        [AliasAs("audio")] StreamPart audio,
        [AliasAs("language")] string? language = null,
        CancellationToken ct = default);

    /// <summary>
    /// Transcribes an audio file and immediately executes the transcribed text as an AI action.
    /// This is the combined voice-to-action endpoint.
    /// </summary>
    /// <param name="audio">The audio file to transcribe (WebM, WAV, MP3, M4A, OGG, FLAC).</param>
    /// <param name="language">Optional BCP-47 language hint (e.g., "es", "en").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The combined transcription and action execution result.</returns>
    [Multipart]
    [Post("/v1/voice/execute")]
    Task<VoiceExecuteResultDto> ExecuteAsync(
        [AliasAs("audio")] StreamPart audio,
        [AliasAs("language")] string? language = null,
        CancellationToken ct = default);
}
