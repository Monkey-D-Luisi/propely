// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Client.Dtos;

/// <summary>
/// Response DTO for a voice transcription result.
/// </summary>
public sealed record TranscriptionResultDto
{
    /// <summary>The transcribed text content.</summary>
    public string Text { get; init; } = null!;

    /// <summary>The detected or specified BCP-47 language code.</summary>
    public string Language { get; init; } = null!;

    /// <summary>The duration of the audio in milliseconds.</summary>
    public int DurationMs { get; init; }
}
