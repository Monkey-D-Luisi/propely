// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AppointmentsApi.Client.Models;

/// <summary>
/// Request to update an existing appointment via the SDK client.
/// </summary>
public sealed record UpdateAppointmentClientRequest
{
    /// <summary>The appointment title.</summary>
    public string Title { get; init; } = null!;

    /// <summary>The appointment type (e.g., PropertyViewing, OwnerMeeting, Generic).</summary>
    public string Type { get; init; } = null!;

    /// <summary>The appointment start time in UTC.</summary>
    public DateTime StartTimeUtc { get; init; }

    /// <summary>The appointment end time in UTC.</summary>
    public DateTime EndTimeUtc { get; init; }

    /// <summary>The appointment description.</summary>
    public string? Description { get; init; }

    /// <summary>The appointment location.</summary>
    public string? Location { get; init; }

    /// <summary>Whether the appointment is an all-day event.</summary>
    public bool? IsAllDay { get; init; }

    /// <summary>The associated property ID (required for PropertyViewing type).</summary>
    public Guid? PropertyId { get; init; }

    /// <summary>The associated contact ID.</summary>
    public Guid? ContactId { get; init; }

    /// <summary>Notes about the appointment.</summary>
    public string? Notes { get; init; }
}
