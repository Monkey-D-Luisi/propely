// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Parameters;

/// <summary>
/// Typed parameters for the CreateContact action.
/// </summary>
public sealed record CreateContactParameters(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Role,
    string? Company,
    string? Notes,
    string? Source) : IActionParameters;
