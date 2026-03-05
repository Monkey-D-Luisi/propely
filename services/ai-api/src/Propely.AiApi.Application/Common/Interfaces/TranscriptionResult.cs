// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Common.Interfaces;

/// <summary>
/// Result of a voice transcription operation.
/// </summary>
/// <param name="Text">The transcribed text content.</param>
/// <param name="Language">The detected or specified BCP-47 language code (e.g., "es", "en").</param>
/// <param name="DurationMs">The duration of the audio in milliseconds.</param>
public sealed record TranscriptionResult(string Text, string Language, int DurationMs);
