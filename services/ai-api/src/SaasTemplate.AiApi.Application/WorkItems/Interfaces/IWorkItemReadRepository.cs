// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Application.Common.Models;
using SaasTemplate.AiApi.Application.WorkItems.Dtos;
using SaasTemplate.AiApi.Domain.WorkItems;

namespace SaasTemplate.AiApi.Application.WorkItems.Interfaces;

/// <summary>
/// Read repository interface for WorkItem queries.
/// Uses the read model (WorkItemsRead) for optimized query performance.
/// </summary>
public interface IWorkItemReadRepository
{
    /// <summary>
    /// Retrieves a WorkItem read model by its unique identifier.
    /// </summary>
    /// <param name="id">The WorkItem identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The WorkItemDto if found; otherwise, null.</returns>
    Task<WorkItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<WorkItemDto>> ListAsync(
        int page,
        int pageSize,
        WorkItemStatus? status,
        string? search,
        CancellationToken cancellationToken);
}
