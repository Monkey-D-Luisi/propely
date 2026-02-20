// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Propely.OrgsApi.Application.Billing;
using Propely.OrgsApi.Application.Billing.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace Propely.OrgsApi.Infrastructure.Billing;

public sealed class StripePaymentService : IPaymentService
{
    private readonly BillingConfiguration _config;
    private readonly StripeClient _stripeClient;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly string _frontendBaseUrl;

    public StripePaymentService(
        IOptions<BillingConfiguration> config,
        ISubscriptionRepository subscriptionRepository,
        ILogger<StripePaymentService> logger,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _config = config.Value;
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
        _stripeClient = new StripeClient(_config.Stripe.SecretKey);
        _frontendBaseUrl = (configuration["Auth:FrontendBaseUrl"] ?? "http://localhost:3000").TrimEnd('/');
    }

    public bool IsEnabled => true;

    public async Task<string> CreateCheckoutSessionAsync(
        Guid orgId,
        string planId,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        var plan = _config.Plans.Find(p => p.Id == planId)
            ?? throw new InvalidOperationException($"Plan '{planId}' not found in configuration.");

        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = plan.StripePriceId,
                    Quantity = 1
                }
            ],
            Metadata = new Dictionary<string, string>
            {
                ["org_id"] = orgId.ToString(),
                ["plan_id"] = planId
            }
        };

        var service = new SessionService(_stripeClient);
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Created Stripe checkout session {SessionId} for org {OrgId} with plan {PlanId}",
            session.Id,
            orgId,
            planId);

        return session.Url;
    }

    public async Task<string> CreatePaymentCheckoutSessionAsync(
        Guid orgId,
        long amount,
        string currency,
        string description,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = currency,
                        UnitAmount = amount,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = description
                        }
                    },
                    Quantity = 1
                }
            ],
            Metadata = new Dictionary<string, string>
            {
                ["org_id"] = orgId.ToString(),
                ["description"] = description
            }
        };

        var service = new SessionService(_stripeClient);
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Created Stripe payment checkout session {SessionId} for org {OrgId}, amount {Amount} {Currency}",
            session.Id,
            orgId,
            amount,
            currency);

        return session.Url;
    }

    public async Task<string> CreateCustomerPortalSessionAsync(
        Guid orgId,
        string returnUrl,
        CancellationToken cancellationToken = default)
    {
        if (!returnUrl.StartsWith(_frontendBaseUrl, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Return URL must belong to the configured frontend domain.");

        var subscription = await _subscriptionRepository.GetByOrgIdAsync(orgId, cancellationToken)
            ?? throw new NotFoundException(
                $"No subscription found for organization {orgId}.");

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = subscription.StripeCustomerId,
            ReturnUrl = returnUrl
        };

        var service = new Stripe.BillingPortal.SessionService(_stripeClient);
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Created Stripe customer portal session for org {OrgId}",
            orgId);

        return session.Url;
    }

    public async Task CancelSubscriptionAsync(
        string stripeSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        var service = new SubscriptionService(_stripeClient);
        await service.CancelAsync(stripeSubscriptionId, cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Cancelled Stripe subscription {SubscriptionId}",
            stripeSubscriptionId);
    }
}
