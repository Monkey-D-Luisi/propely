// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Tools;

/// <summary>
/// Registry of all provider-neutral tool schemas available in the AI Action Engine.
/// </summary>
public interface IToolSchemaRegistry
{
    /// <summary>All registered tool schemas.</summary>
    IReadOnlyList<ToolSchema> All { get; }

    /// <summary>Gets a tool schema by function name, or null if not found.</summary>
    ToolSchema? GetByName(string name);

    /// <summary>Resolves a function name to its <see cref="ActionType"/>. Returns <see cref="ActionType.Unknown"/> if not found.</summary>
    ActionType ResolveActionType(string functionName);
}
