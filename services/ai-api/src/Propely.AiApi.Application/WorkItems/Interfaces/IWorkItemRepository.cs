// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.WorkItems;

namespace Propely.AiApi.Application.WorkItems.Interfaces;

/// <summary>
/// Repository interface for WorkItem aggregate persistence.
/// </summary>
public interface IWorkItemRepository
{
    /// <summary>
    /// Adds a new WorkItem to the repository.
    /// </summary>
    /// <param name="workItem">The WorkItem to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(WorkItem workItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a WorkItem by its unique identifier.
    /// </summary>
    /// <param name="id">The WorkItem identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The WorkItem if found; otherwise, null.</returns>
    Task<WorkItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing WorkItem in the repository.
    /// </summary>
    /// <param name="workItem">The WorkItem to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(WorkItem workItem, CancellationToken cancellationToken = default);
}
