// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Models;

/// <summary>
/// Represents a single exchange in a conversation: user input → action result.
/// </summary>
public sealed record ConversationExchange(
    string UserText,
    ActionType ActionType,
    string ResultMessage,
    bool Success,
    DateTimeOffset Timestamp);
