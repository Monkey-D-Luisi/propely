// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Tools;

/// <summary>
/// Converts provider-neutral <see cref="ToolSchema"/> instances into a provider-specific tool format.
/// Each LLM provider (OpenAI, Claude, Gemini) implements this interface.
/// </summary>
public interface IToolAdapter
{
    /// <summary>Converts all schemas into the provider-specific tool format.</summary>
    IReadOnlyList<object> ConvertAll(IReadOnlyList<ToolSchema> schemas);
}
