// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Parameters;

/// <summary>
/// Typed parameters for the QueryAppointments action.
/// </summary>
public sealed record QueryAppointmentsParameters(
    string? Status,
    string? Type,
    Guid? PropertyId,
    DateTime? FromDate,
    DateTime? ToDate) : IActionParameters;
