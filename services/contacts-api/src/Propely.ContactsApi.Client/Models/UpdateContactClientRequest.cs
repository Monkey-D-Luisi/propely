// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.ContactsApi.Client.Models;

/// <summary>
/// Request to update an existing contact via the SDK client.
/// </summary>
public sealed record UpdateContactClientRequest
{
    /// <summary>The contact's first name.</summary>
    public string? FirstName { get; init; }

    /// <summary>The contact's last name.</summary>
    public string? LastName { get; init; }

    /// <summary>The contact's email address.</summary>
    public string? Email { get; init; }

    /// <summary>The contact's roles.</summary>
    public IReadOnlyCollection<string>? Roles { get; init; }

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
}
