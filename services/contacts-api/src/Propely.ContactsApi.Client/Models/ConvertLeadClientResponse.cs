// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.ContactsApi.Client.Models;

/// <summary>
/// Response from converting a lead into a contact.
/// </summary>
public sealed record ConvertLeadClientResponse
{
    /// <summary>The updated lead.</summary>
    public LeadResponse Lead { get; init; } = null!;

    /// <summary>The created or updated contact.</summary>
    public ContactResponse Contact { get; init; } = null!;

    /// <summary>Whether a new contact was created (true) or an existing one was merged (false).</summary>
    public bool WasNewContact { get; init; }
}
