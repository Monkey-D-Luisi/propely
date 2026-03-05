// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Client.Dtos;

/// <summary>
/// Response DTO for the voice execute endpoint.
/// Contains both the transcription result and the action execution result.
/// </summary>
public sealed record VoiceExecuteResultDto
{
    /// <summary>The text transcribed from the audio input.</summary>
    public string TranscribedText { get; init; } = null!;

    /// <summary>The detected or specified BCP-47 language code.</summary>
    public string Language { get; init; } = null!;

    /// <summary>The duration of the audio in milliseconds.</summary>
    public int DurationMs { get; init; }

    /// <summary>The result of the AI action execution.</summary>
    public ActionResultDto Action { get; init; } = null!;
}
