// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.ContactsApi.Client.Models;

/// <summary>
/// Full contact details returned by the Contacts API.
/// </summary>
public sealed record ContactResponse
{
    /// <summary>The unique contact identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>The contact's first name.</summary>
    public string FirstName { get; init; } = null!;

    /// <summary>The contact's last name.</summary>
    public string LastName { get; init; } = null!;

    /// <summary>The contact's email address.</summary>
    public string Email { get; init; } = null!;

    /// <summary>The contact's primary phone number.</summary>
    public string? Phone { get; init; }

    /// <summary>The contact's secondary phone number.</summary>
    public string? SecondaryPhone { get; init; }

    /// <summary>The contact's company name.</summary>
    public string? Company { get; init; }

    /// <summary>Notes about the contact.</summary>
    public string? Notes { get; init; }

    /// <summary>The contact's preferred language.</summary>
    public string? PreferredLanguage { get; init; }

    /// <summary>The source of the contact.</summary>
    public string? Source { get; init; }

    /// <summary>The assigned agent's user ID.</summary>
    public Guid? AssignedAgentId { get; init; }

    /// <summary>The tenant (organization) ID.</summary>
    public Guid TenantId { get; init; }

    /// <summary>The contact's roles.</summary>
    public IReadOnlyCollection<string> Roles { get; init; } = [];

    /// <summary>The contact's property interests.</summary>
    public IReadOnlyCollection<PropertyInterestResponse> PropertyInterests { get; init; } = [];

    /// <summary>Date the contact was created (UTC).</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Date the contact was last updated (UTC).</summary>
    public DateTime? UpdatedAtUtc { get; init; }
}

/// <summary>
/// Property interest details.
/// </summary>
public sealed record PropertyInterestResponse
{
    /// <summary>The unique interest identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>The property ID.</summary>
    public Guid PropertyId { get; init; }

    /// <summary>The type of interest (Buying, Renting, Selling).</summary>
    public string InterestType { get; init; } = null!;

    /// <summary>Notes about the interest.</summary>
    public string? Notes { get; init; }

    /// <summary>Date the interest was created (UTC).</summary>
    public DateTime CreatedAtUtc { get; init; }
}
