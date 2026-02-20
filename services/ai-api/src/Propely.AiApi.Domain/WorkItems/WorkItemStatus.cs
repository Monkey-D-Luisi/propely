// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Domain.WorkItems;

/// <summary>
/// Represents the lifecycle status of a WorkItem.
/// </summary>
public enum WorkItemStatus
{
    Pending = 0,
    Active = 1,
    Deleted = 2,
    Deactivated = 3,
    Expired = 4
}
