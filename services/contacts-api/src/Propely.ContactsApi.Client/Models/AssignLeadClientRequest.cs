// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.ContactsApi.Client.Models;

/// <summary>
/// Request to assign an agent to a lead via the SDK client.
/// </summary>
public sealed record AssignLeadClientRequest
{
    /// <summary>The agent ID to assign.</summary>
    public Guid AgentId { get; init; }
}
