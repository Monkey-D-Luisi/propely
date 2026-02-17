// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.OrgsApi.Application.Billing.Interfaces;

public interface IPaymentService
{
    Task<string> CreateCheckoutSessionAsync(
        Guid orgId,
        string planId,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default);

    Task<string> CreatePaymentCheckoutSessionAsync(
        Guid orgId,
        long amount,
        string currency,
        string description,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default);

    Task<string> CreateCustomerPortalSessionAsync(
        Guid orgId,
        string returnUrl,
        CancellationToken cancellationToken = default);

    Task CancelSubscriptionAsync(
        string stripeSubscriptionId,
        CancellationToken cancellationToken = default);

    bool IsEnabled { get; }
}
