// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Tools;

/// <summary>
/// A provider-neutral tool schema representing an AI action tool.
/// Contains only the JSON Schema definition without any vendor-specific types.
/// </summary>
/// <param name="Name">The function name in snake_case (e.g., "create_property").</param>
/// <param name="Description">Human-readable description of what the tool does.</param>
/// <param name="ParametersJsonSchema">Raw JSON Schema string defining the tool's input parameters.</param>
/// <param name="ActionType">The <see cref="ActionType"/> this tool maps to.</param>
public sealed record ToolSchema(
    string Name,
    string Description,
    string ParametersJsonSchema,
    ActionType ActionType);
