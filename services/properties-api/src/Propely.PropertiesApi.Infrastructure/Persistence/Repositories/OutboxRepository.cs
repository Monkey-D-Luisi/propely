// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Propely.PropertiesApi.Infrastructure.Persistence.Repositories;

public sealed class OutboxRepository : IOutboxRepository
{
    private readonly AppDbContext _context;

    public OutboxRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _context.OutboxMessages.AddAsync(message, cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.OutboxMessages
            .FromSqlRaw(
                @"SELECT * FROM outbox_messages
                  WHERE processed_at_utc IS NULL
                  ORDER BY occurred_at_utc
                  LIMIT {0}
                  FOR UPDATE SKIP LOCKED",
                batchSize)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _context.OutboxMessages.Update(message);
        return Task.CompletedTask;
    }

    public Task<int> CountUnprocessedMessagesAsync(CancellationToken cancellationToken = default)
    {
        return _context.OutboxMessages.CountAsync(m => m.ProcessedAtUtc == null, cancellationToken);
    }
}
