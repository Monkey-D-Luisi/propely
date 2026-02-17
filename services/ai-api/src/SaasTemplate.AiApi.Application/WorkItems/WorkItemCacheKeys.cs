// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.AiApi.Application.WorkItems;

/// <summary>
/// Centralized cache key generation for WorkItem entities.
/// </summary>
public static class WorkItemCacheKeys
{
    /// <summary>
    /// Gets the cache key for a work item by ID.
    /// </summary>
    /// <param name="workItemId">The work item ID.</param>
    /// <returns>The cache key in format "workitem:{id}".</returns>
    public static string GetById(Guid workItemId) => $"workitem:{workItemId}";
}
