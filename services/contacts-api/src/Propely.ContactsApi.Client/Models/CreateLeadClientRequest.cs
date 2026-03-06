// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.ContactsApi.Client.Models;

/// <summary>
/// Request to create a new lead via the SDK client.
/// </summary>
public sealed record CreateLeadClientRequest
{
    /// <summary>The lead's name.</summary>
    public string Name { get; init; } = null!;

    /// <summary>The lead's email address.</summary>
    public string Email { get; init; } = null!;

    /// <summary>The property ID the lead is interested in.</summary>
    public Guid PropertyId { get; init; }

    /// <summary>The lead's phone number.</summary>
    public string? Phone { get; init; }

    /// <summary>The lead's message or inquiry.</summary>
    public string? Message { get; init; }

    /// <summary>The source of the lead.</summary>
    public string? Source { get; init; }

    /// <summary>The agent ID to assign the lead to.</summary>
    public Guid? AssignedAgentId { get; init; }
}
