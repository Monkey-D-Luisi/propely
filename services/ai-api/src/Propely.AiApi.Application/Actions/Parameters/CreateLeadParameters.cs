// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Parameters;

/// <summary>
/// Typed parameters for the CreateLead action.
/// </summary>
public sealed record CreateLeadParameters(
    string? Name,
    string? Email,
    string? Phone,
    string? Message,
    string? Source,
    Guid? PropertyId) : IActionParameters;
