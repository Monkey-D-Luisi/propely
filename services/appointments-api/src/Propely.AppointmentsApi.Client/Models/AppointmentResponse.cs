// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AppointmentsApi.Client.Models;

/// <summary>
/// Full appointment details returned by the Appointments API.
/// </summary>
public sealed record AppointmentResponse
{
    /// <summary>The unique appointment identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>The appointment title.</summary>
    public string Title { get; init; } = null!;

    /// <summary>The appointment description.</summary>
    public string? Description { get; init; }

    /// <summary>The appointment type (e.g., PropertyViewing, OwnerMeeting, Generic).</summary>
    public string Type { get; init; } = null!;

    /// <summary>The appointment status (e.g., Scheduled, Confirmed, Completed, Cancelled, NoShow).</summary>
    public string Status { get; init; } = null!;

    /// <summary>The appointment start time in UTC.</summary>
    public DateTime StartTimeUtc { get; init; }

    /// <summary>The appointment end time in UTC.</summary>
    public DateTime EndTimeUtc { get; init; }

    /// <summary>The appointment location.</summary>
    public string? Location { get; init; }

    /// <summary>Whether the appointment is an all-day event.</summary>
    public bool IsAllDay { get; init; }

    /// <summary>The associated property ID (required for PropertyViewing type).</summary>
    public Guid? PropertyId { get; init; }

    /// <summary>The associated contact ID.</summary>
    public Guid? ContactId { get; init; }

    /// <summary>The agent (user) ID who owns the appointment.</summary>
    public Guid AgentId { get; init; }

    /// <summary>The tenant (organization) ID.</summary>
    public Guid TenantId { get; init; }

    /// <summary>The reason for cancellation, if applicable.</summary>
    public string? CancellationReason { get; init; }

    /// <summary>Notes about the appointment.</summary>
    public string? Notes { get; init; }

    /// <summary>Date the appointment was created (UTC).</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Date the appointment was last updated (UTC).</summary>
    public DateTime? UpdatedAtUtc { get; init; }
}
