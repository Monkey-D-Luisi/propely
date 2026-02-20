// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Commands.ProcessWebhookEvent;

namespace Propely.OrgsApi.Application.Billing.Interfaces;

/// <summary>
/// Abstracts Stripe webhook event mapping so the Api layer does not depend
/// directly on Infrastructure.Billing.WebhookEventMapper.
/// </summary>
public interface IWebhookEventMapper
{
    WebhookEventData MapToEventData(
        object stripeEvent,
        IReadOnlyDictionary<string, string>? priceIdToPlanId = null);
}
