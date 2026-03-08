// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Parameters;

/// <summary>
/// Typed parameters for the QueryLeads action.
/// </summary>
public sealed record QueryLeadsParameters(
    string? Status,
    Guid? PropertyId,
    string? Search) : IActionParameters;
