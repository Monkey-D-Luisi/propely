// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json.Serialization;

namespace SaasTemplate.OrgsApi.Application.Billing.Commands.ProcessWebhookEvent;

public sealed record WebhookEventData
{
    [JsonPropertyName("orgId")]
    public Guid? OrgId { get; init; }

    [JsonPropertyName("stripeCustomerId")]
    public string? StripeCustomerId { get; init; }

    [JsonPropertyName("stripeSubscriptionId")]
    public string? StripeSubscriptionId { get; init; }

    [JsonPropertyName("planId")]
    public string? PlanId { get; init; }

    [JsonPropertyName("stripeStatus")]
    public string? StripeStatus { get; init; }

    [JsonPropertyName("currentPeriodEnd")]
    public DateTime? CurrentPeriodEnd { get; init; }

    [JsonPropertyName("stripePaymentIntentId")]
    public string? StripePaymentIntentId { get; init; }

    [JsonPropertyName("amount")]
    public long? Amount { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("checkoutMode")]
    public string? CheckoutMode { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
