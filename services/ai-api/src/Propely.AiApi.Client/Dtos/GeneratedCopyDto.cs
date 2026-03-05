// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Client.Dtos;

/// <summary>
/// Marketing copy generated in multiple language variants with the applied tone.
/// </summary>
public sealed record GeneratedCopyDto
{
    /// <summary>
    /// Language code to generated text mapping (e.g., "es" -> "Amplio apartamento...").
    /// </summary>
    public Dictionary<string, string> Variants { get; init; } = new();

    /// <summary>
    /// The tone used for generation (e.g., "professional", "luxury", "casual", "concise").
    /// </summary>
    public string Tone { get; init; } = string.Empty;
}
