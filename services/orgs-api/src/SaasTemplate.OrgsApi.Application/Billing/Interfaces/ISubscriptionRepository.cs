// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Domain.Billing;

namespace SaasTemplate.OrgsApi.Application.Billing.Interfaces;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByOrgIdAsync(Guid orgId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
}
