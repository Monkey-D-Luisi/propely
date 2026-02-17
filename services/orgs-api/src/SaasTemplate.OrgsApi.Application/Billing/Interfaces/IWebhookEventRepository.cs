// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.OrgsApi.Application.Billing.Interfaces;

public interface IWebhookEventRepository
{
    Task<bool> IsProcessedAsync(string eventId, CancellationToken cancellationToken = default);
    Task MarkProcessedAsync(string eventId, string eventType, CancellationToken cancellationToken = default);
}
