// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Api.Dtos;

/// <summary>
/// Response DTO for a voice transcription result.
/// </summary>
/// <param name="Text">The transcribed text content.</param>
/// <param name="Language">The detected or specified BCP-47 language code.</param>
/// <param name="DurationMs">The duration of the audio in milliseconds.</param>
public sealed record TranscriptionResultDto(string Text, string Language, int DurationMs);
