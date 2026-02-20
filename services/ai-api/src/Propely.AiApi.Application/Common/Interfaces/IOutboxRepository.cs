// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.Common.Models;

namespace Propely.AiApi.Application.Common.Interfaces;

/// <summary>
/// Repository interface for outbox message persistence.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Adds a new outbox message to the repository.
    /// </summary>
    /// <param name="message">The outbox message to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unprocessed messages from the outbox ordered by occurrence time.
    /// </summary>
    /// <param name="batchSize">Maximum number of messages to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of unprocessed outbox messages.</returns>
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing outbox message (e.g., to mark as processed).
    /// </summary>
    /// <param name="message">The outbox message to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of unprocessed messages in the outbox.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of unprocessed outbox messages.</returns>
    Task<int> CountUnprocessedMessagesAsync(CancellationToken cancellationToken = default);
}
