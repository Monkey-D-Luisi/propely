// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Api.Dtos;

/// <summary>
/// Response DTO for the voice execute endpoint.
/// Contains both the transcription result and the action execution result.
/// </summary>
/// <param name="TranscribedText">The text transcribed from the audio input.</param>
/// <param name="Language">The detected or specified BCP-47 language code.</param>
/// <param name="DurationMs">The duration of the audio in milliseconds.</param>
/// <param name="Action">The result of the AI action execution.</param>
public sealed record VoiceExecuteResultDto(
    string TranscribedText,
    string Language,
    int DurationMs,
    ActionResultDto Action);
