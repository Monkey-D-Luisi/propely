// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Logging;
using SaasTemplate.OrgsApi.Application.Billing.Interfaces;

namespace SaasTemplate.OrgsApi.Infrastructure.Billing;

public sealed class NoOpPaymentService : IPaymentService
{
    private readonly ILogger<NoOpPaymentService> _logger;

    public NoOpPaymentService(ILogger<NoOpPaymentService> logger)
    {
        _logger = logger;
    }

    public bool IsEnabled => false;

    public Task<string> CreateCheckoutSessionAsync(
        Guid orgId,
        string planId,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Billing disabled (BILLING_MODE=free). Skipping checkout session for org {OrgId}",
            orgId);
        return Task.FromResult(string.Empty);
    }

    public Task<string> CreatePaymentCheckoutSessionAsync(
        Guid orgId,
        long amount,
        string currency,
        string description,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Billing disabled (BILLING_MODE=free). Skipping payment checkout session for org {OrgId}",
            orgId);
        return Task.FromResult(string.Empty);
    }

    public Task<string> CreateCustomerPortalSessionAsync(
        Guid orgId,
        string returnUrl,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Billing disabled (BILLING_MODE=free). Skipping customer portal session for org {OrgId}",
            orgId);
        return Task.FromResult(string.Empty);
    }

    public Task CancelSubscriptionAsync(
        string stripeSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Billing disabled (BILLING_MODE=free). Skipping subscription cancellation for {SubscriptionId}",
            stripeSubscriptionId);
        return Task.CompletedTask;
    }
}
