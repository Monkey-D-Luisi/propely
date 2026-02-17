// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Billing.Interfaces;
using SaasTemplate.OrgsApi.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace SaasTemplate.OrgsApi.Infrastructure.Persistence.Repositories;

public sealed class WebhookEventRepository : IWebhookEventRepository
{
    private readonly AppDbContext _context;

    public WebhookEventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsProcessedAsync(string eventId, CancellationToken cancellationToken = default)
    {
        return await _context.ProcessedWebhookEvents
            .AnyAsync(e => e.EventId == eventId, cancellationToken);
    }

    public async Task MarkProcessedAsync(string eventId, string eventType, CancellationToken cancellationToken = default)
    {
        var record = ProcessedWebhookEvent.Create(eventId, eventType);
        await _context.ProcessedWebhookEvents.AddAsync(record, cancellationToken);
    }
}
