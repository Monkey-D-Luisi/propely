// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Commands.ProcessWebhookEvent;
using Propely.OrgsApi.Application.Billing.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace Propely.OrgsApi.Infrastructure.Billing;

/// <summary>
/// Maps Stripe webhook event objects to application-layer WebhookEventData DTOs.
/// Implements IWebhookEventMapper so the Api layer can depend on the abstraction
/// instead of this Infrastructure class directly.
/// </summary>
public sealed class WebhookEventMapper : IWebhookEventMapper
{
    public WebhookEventData MapToEventData(
        object stripeEvent,
        IReadOnlyDictionary<string, string>? priceIdToPlanId = null)
    {
        if (stripeEvent is not Event typed)
            return new WebhookEventData();

        return MapToEventDataStatic(typed, priceIdToPlanId);
    }

    public static WebhookEventData MapToEventDataStatic(
        Event stripeEvent,
        IReadOnlyDictionary<string, string>? priceIdToPlanId = null)
    {
        return stripeEvent.Type switch
        {
            "checkout.session.completed" => MapCheckoutSession(stripeEvent),
            "invoice.paid" or "invoice.payment_failed" => MapInvoice(stripeEvent),
            "customer.subscription.updated" or "customer.subscription.deleted"
                => MapSubscription(stripeEvent, priceIdToPlanId),
            _ => new WebhookEventData()
        };
    }

    private static WebhookEventData MapCheckoutSession(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Session session)
            return new WebhookEventData();

        Guid? orgId = session.Metadata.TryGetValue("org_id", out var orgIdStr)
            && Guid.TryParse(orgIdStr, out var parsed)
                ? parsed
                : null;

        string? planId = session.Metadata.TryGetValue("plan_id", out var plan) ? plan : null;
        string? description = session.Metadata.TryGetValue("description", out var desc) ? desc : null;

        return new WebhookEventData
        {
            OrgId = orgId,
            StripeCustomerId = session.CustomerId,
            StripeSubscriptionId = session.SubscriptionId,
            PlanId = planId,
            CheckoutMode = session.Mode,
            StripePaymentIntentId = session.PaymentIntentId,
            Amount = session.AmountTotal,
            Currency = session.Currency,
            Description = description
        };
    }

    private static WebhookEventData MapInvoice(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Invoice invoice)
            return new WebhookEventData();

        var subscriptionId = invoice.Parent?.SubscriptionDetails?.SubscriptionId;
        var periodEnd = invoice.Lines?.Data?.FirstOrDefault()?.Period?.End;

        return new WebhookEventData
        {
            StripeSubscriptionId = subscriptionId,
            CurrentPeriodEnd = periodEnd
        };
    }

    private static WebhookEventData MapSubscription(
        Event stripeEvent,
        IReadOnlyDictionary<string, string>? priceIdToPlanId)
    {
        if (stripeEvent.Data.Object is not Stripe.Subscription subscription)
            return new WebhookEventData();

        var stripePriceId = subscription.Items?.Data?.FirstOrDefault()?.Price?.Id;
        string? planId = null;
        if (stripePriceId is not null && priceIdToPlanId is not null)
            priceIdToPlanId.TryGetValue(stripePriceId, out planId);

        var periodEnd = subscription.Items?.Data?.FirstOrDefault()?.CurrentPeriodEnd;

        return new WebhookEventData
        {
            StripeSubscriptionId = subscription.Id,
            StripeStatus = subscription.Status,
            PlanId = planId,
            CurrentPeriodEnd = periodEnd
        };
    }
}
