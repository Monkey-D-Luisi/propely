// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.ContactsApi.Client.Models;

/// <summary>
/// Request to change the status of a lead via the SDK client.
/// </summary>
public sealed record ChangeStatusClientRequest
{
    /// <summary>The new status (New, Contacted, Qualified, Converted, Lost).</summary>
    public string Status { get; init; } = null!;
}
