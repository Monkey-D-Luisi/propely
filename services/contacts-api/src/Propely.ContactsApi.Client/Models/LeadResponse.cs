// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.ContactsApi.Client.Models;

/// <summary>
/// Full lead details returned by the Contacts API.
/// </summary>
public sealed record LeadResponse
{
    /// <summary>The unique lead identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>The lead's name.</summary>
    public string Name { get; init; } = null!;

    /// <summary>The lead's email address.</summary>
    public string Email { get; init; } = null!;

    /// <summary>The lead's phone number.</summary>
    public string? Phone { get; init; }

    /// <summary>The lead's message or inquiry.</summary>
    public string? Message { get; init; }

    /// <summary>The source of the lead.</summary>
    public string? Source { get; init; }

    /// <summary>The property ID the lead is interested in.</summary>
    public Guid PropertyId { get; init; }

    /// <summary>The tenant (organization) ID.</summary>
    public Guid TenantId { get; init; }

    /// <summary>The current lead status (New, Contacted, Qualified, Converted, Lost).</summary>
    public string Status { get; init; } = null!;

    /// <summary>The assigned agent's user ID.</summary>
    public Guid? AssignedAgentId { get; init; }

    /// <summary>The contact ID if the lead has been converted.</summary>
    public Guid? ContactId { get; init; }

    /// <summary>Date the lead was created (UTC).</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Date the lead was last updated (UTC).</summary>
    public DateTime? UpdatedAtUtc { get; init; }
}
