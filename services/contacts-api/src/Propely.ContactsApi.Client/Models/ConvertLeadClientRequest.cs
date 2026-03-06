// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.ContactsApi.Client.Models;

/// <summary>
/// Request to convert a qualified lead into a contact via the SDK client.
/// </summary>
public sealed record ConvertLeadClientRequest
{
    /// <summary>The role to assign to the new contact (e.g., Buyer, Seller, Tenant, Landlord, Professional).</summary>
    public string Role { get; init; } = "Buyer";

    /// <summary>Optional notes for the conversion.</summary>
    public string? Notes { get; init; }
}
