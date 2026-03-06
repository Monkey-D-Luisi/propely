// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AppointmentsApi.Client.Models;

/// <summary>
/// Request to complete an appointment via the SDK client.
/// </summary>
public sealed record CompleteAppointmentClientRequest
{
    /// <summary>Optional notes to add upon completion.</summary>
    public string? Notes { get; init; }
}
