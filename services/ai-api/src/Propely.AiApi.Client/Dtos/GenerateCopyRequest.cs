// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Client.Dtos;

/// <summary>
/// Request to generate marketing copy for a property listing.
/// </summary>
public sealed record GenerateCopyRequest
{
    /// <summary>Text description of the property to generate copy for.</summary>
    public string PropertyData { get; init; } = null!;

    /// <summary>The tone to use (e.g., "professional", "luxury", "casual", "concise").</summary>
    public string Tone { get; init; } = "professional";

    /// <summary>Language codes to generate copy in (e.g., ["es", "en", "pt"]).</summary>
    public List<string> Languages { get; init; } = [];
}
