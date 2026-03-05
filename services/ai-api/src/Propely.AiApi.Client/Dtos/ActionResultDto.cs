// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Client.Dtos;

/// <summary>
/// Response containing the result of an AI action execution.
/// </summary>
public sealed record ActionResultDto
{
    /// <summary>Whether the action was executed successfully.</summary>
    public bool Success { get; init; }

    /// <summary>The classified action type (e.g., CreateProperty, QueryProperties).</summary>
    public string ActionType { get; init; } = null!;

    /// <summary>Optional action-specific data payload.</summary>
    public object? Data { get; init; }

    /// <summary>Human-readable confirmation or error message.</summary>
    public string Message { get; init; } = null!;

    /// <summary>List of error messages, if any.</summary>
    public string[] Errors { get; init; } = [];

    /// <summary>Confidence score of the intent classification (0.0 to 1.0).</summary>
    public double Confidence { get; init; }
}
