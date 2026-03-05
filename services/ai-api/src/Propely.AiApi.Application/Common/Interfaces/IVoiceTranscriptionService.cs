// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Common.Interfaces;

/// <summary>
/// Interface for voice-to-text transcription services.
/// </summary>
public interface IVoiceTranscriptionService
{
    /// <summary>
    /// Transcribes an audio stream into text.
    /// </summary>
    /// <param name="audio">The audio stream to transcribe.</param>
    /// <param name="fileName">The original filename (used to detect format).</param>
    /// <param name="languageHint">Optional BCP-47 language hint (e.g., "es", "en").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="TranscriptionResult"/> with the transcribed text, detected language, and duration.</returns>
    Task<TranscriptionResult> TranscribeAsync(Stream audio, string fileName, string? languageHint, CancellationToken ct);
}
