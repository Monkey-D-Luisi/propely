// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Models;

/// <summary>
/// Configuration options for conversation context behavior.
/// </summary>
public sealed class ConversationContextOptions
{
    public const string SectionName = "ConversationContext";

    /// <summary>
    /// Maximum number of exchanges to keep in the conversation window.
    /// </summary>
    public int MaxExchanges { get; set; } = 10;

    /// <summary>
    /// Session time-to-live in minutes. Sessions expire after this duration of inactivity.
    /// </summary>
    public int SessionTtlMinutes { get; set; } = 30;
}
