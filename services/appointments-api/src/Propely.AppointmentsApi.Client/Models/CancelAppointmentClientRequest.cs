// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AppointmentsApi.Client.Models;

/// <summary>
/// Request to cancel an appointment via the SDK client.
/// </summary>
public sealed record CancelAppointmentClientRequest
{
    /// <summary>The reason for cancelling the appointment.</summary>
    public string Reason { get; init; } = null!;
}
