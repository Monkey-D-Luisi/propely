// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Billing;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Propely.OrgsApi.Application.Billing.Commands.ProcessWebhookEvent;

public sealed class ProcessWebhookEventCommandHandler : IRequestHandler<ProcessWebhookEventCommand>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWebhookEventRepository _webhookEventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ILogger<ProcessWebhookEventCommandHandler> _logger;

    public ProcessWebhookEventCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IPaymentRepository paymentRepository,
        IWebhookEventRepository webhookEventRepository,
        IUnitOfWork unitOfWork,
        IMembershipRepository membershipRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        IOrganizationRepository organizationRepository,
        ILogger<ProcessWebhookEventCommandHandler> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _paymentRepository = paymentRepository;
        _webhookEventRepository = webhookEventRepository;
        _unitOfWork = unitOfWork;
        _membershipRepository = membershipRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _organizationRepository = organizationRepository;
        _logger = logger;
    }

    public async Task Handle(ProcessWebhookEventCommand request, CancellationToken cancellationToken)
    {
        if (await _webhookEventRepository.IsProcessedAsync(request.EventId, cancellationToken))
        {
            _logger.LogInformation("Webhook event {EventId} already processed, skipping", request.EventId);
            return;
        }

        switch (request.EventType)
        {
            case "checkout.session.completed":
                await HandleCheckoutSessionCompleted(request, cancellationToken);
                break;

            case "invoice.paid":
                await HandleInvoicePaid(request, cancellationToken);
                break;

            case "invoice.payment_failed":
                await HandleInvoicePaymentFailed(request, cancellationToken);
                break;

            case "customer.subscription.updated":
                await HandleSubscriptionUpdated(request, cancellationToken);
                break;

            case "customer.subscription.deleted":
                await HandleSubscriptionDeleted(request, cancellationToken);
                break;

            default:
                _logger.LogDebug("Unhandled webhook event type {EventType}, skipping", request.EventType);
                return;
        }

        await _webhookEventRepository.MarkProcessedAsync(request.EventId, request.EventType, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
            when (ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true
              || ex.InnerException?.Message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogInformation(
                "Webhook event {EventId} was concurrently processed by another request, skipping",
                request.EventId);
            return;
        }

        _logger.LogInformation(
            "Processed webhook event {EventId} of type {EventType}",
            request.EventId,
            request.EventType);
    }

    private async Task HandleCheckoutSessionCompleted(ProcessWebhookEventCommand request, CancellationToken cancellationToken)
    {
        var data = request.EventData;

        if (data.CheckoutMode == "payment")
        {
            await HandlePaymentCheckoutCompleted(data, request.EventId, cancellationToken);
            return;
        }

        if (data.OrgId is null || data.StripeCustomerId is null || data.StripeSubscriptionId is null)
        {
            _logger.LogWarning(
                "checkout.session.completed event {EventId} missing required fields (org_id, customer, subscription)",
                request.EventId);
            return;
        }

        var existing = await _subscriptionRepository.GetByOrgIdAsync(data.OrgId.Value, cancellationToken);

        if (existing is not null)
        {
            existing.UpdateStatus(SubscriptionStatus.Active, data.CurrentPeriodEnd ?? DateTime.UtcNow.AddMonths(1));
            if (!string.IsNullOrWhiteSpace(data.PlanId))
                existing.UpdatePlan(data.PlanId);
        }
        else
        {
            var subscription = Subscription.Create(
                data.OrgId.Value,
                data.StripeCustomerId,
                data.StripeSubscriptionId,
                data.PlanId ?? "unknown",
                SubscriptionStatus.Active,
                data.CurrentPeriodEnd ?? DateTime.UtcNow.AddMonths(1));

            await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        }
    }

    private async Task HandlePaymentCheckoutCompleted(WebhookEventData data, string eventId, CancellationToken cancellationToken)
    {
        if (data.OrgId is null || data.StripePaymentIntentId is null)
        {
            _logger.LogWarning(
                "checkout.session.completed (payment) event {EventId} missing required fields (org_id, payment_intent)",
                eventId);
            return;
        }

        if (data.Amount is null || string.IsNullOrWhiteSpace(data.Currency))
        {
            _logger.LogWarning(
                "checkout.session.completed (payment) event {EventId} missing required fields (amount, currency)",
                eventId);
            return;
        }

        var existing = await _paymentRepository.GetByStripePaymentIntentIdAsync(
            data.StripePaymentIntentId, cancellationToken);

        if (existing is not null)
        {
            _logger.LogInformation(
                "Payment for PaymentIntent {PaymentIntentId} already exists, skipping",
                data.StripePaymentIntentId);
            return;
        }

        var payment = Payment.Create(
            data.OrgId.Value,
            data.StripePaymentIntentId,
            data.Amount.Value,
            data.Currency,
            data.Description ?? "One-time payment",
            PaymentStatus.Succeeded);

        await _paymentRepository.AddAsync(payment, cancellationToken);
    }

    private async Task HandleInvoicePaid(ProcessWebhookEventCommand request, CancellationToken cancellationToken)
    {
        var data = request.EventData;

        if (data.StripeSubscriptionId is null) return;

        var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(
            data.StripeSubscriptionId, cancellationToken);

        if (subscription is null)
        {
            _logger.LogWarning(
                "invoice.paid event {EventId}: subscription {StripeSubscriptionId} not found",
                request.EventId,
                data.StripeSubscriptionId);
            return;
        }

        subscription.UpdateStatus(
            SubscriptionStatus.Active,
            data.CurrentPeriodEnd ?? DateTime.UtcNow.AddMonths(1));
    }

    private async Task HandleInvoicePaymentFailed(ProcessWebhookEventCommand request, CancellationToken cancellationToken)
    {
        var data = request.EventData;

        if (data.StripeSubscriptionId is null) return;

        var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(
            data.StripeSubscriptionId, cancellationToken);

        if (subscription is null)
        {
            _logger.LogWarning(
                "invoice.payment_failed event {EventId}: subscription {StripeSubscriptionId} not found",
                request.EventId,
                data.StripeSubscriptionId);
            return;
        }

        subscription.UpdateStatus(SubscriptionStatus.PastDue, subscription.CurrentPeriodEnd);

        await SendPaymentFailureNotification(subscription.OrganizationId, cancellationToken);
    }

    private async Task HandleSubscriptionUpdated(ProcessWebhookEventCommand request, CancellationToken cancellationToken)
    {
        var data = request.EventData;

        if (data.StripeSubscriptionId is null) return;

        var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(
            data.StripeSubscriptionId, cancellationToken);

        if (subscription is null)
        {
            _logger.LogWarning(
                "customer.subscription.updated event {EventId}: subscription {StripeSubscriptionId} not found",
                request.EventId,
                data.StripeSubscriptionId);
            return;
        }

        var status = MapStripeStatus(data.StripeStatus);
        subscription.UpdateStatus(status, data.CurrentPeriodEnd ?? subscription.CurrentPeriodEnd);

        if (!string.IsNullOrWhiteSpace(data.PlanId))
            subscription.UpdatePlan(data.PlanId);
    }

    private async Task HandleSubscriptionDeleted(ProcessWebhookEventCommand request, CancellationToken cancellationToken)
    {
        var data = request.EventData;

        if (data.StripeSubscriptionId is null) return;

        var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(
            data.StripeSubscriptionId, cancellationToken);

        if (subscription is null)
        {
            _logger.LogWarning(
                "customer.subscription.deleted event {EventId}: subscription {StripeSubscriptionId} not found",
                request.EventId,
                data.StripeSubscriptionId);
            return;
        }

        subscription.Cancel();
    }

    private async Task SendPaymentFailureNotification(Guid organizationId, CancellationToken cancellationToken)
    {
        var org = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);
        if (org is null) return;

        var memberships = await _membershipRepository.GetByOrgIdAsync(organizationId, cancellationToken);
        var ownerUserIds = memberships
            .Where(m => m.Role == Domain.Organizations.MembershipRole.Owner)
            .Select(m => m.UserId)
            .ToList();

        if (ownerUserIds.Count == 0) return;

        var owners = await _userRepository.GetByIdsAsync(ownerUserIds, cancellationToken);

        foreach (var user in owners)
        {
            if (user.Email is not null)
            {
                await _emailService.SendPaymentFailureEmailAsync(
                    user.Email, org.Name, cancellationToken);
            }
        }
    }

    private static SubscriptionStatus MapStripeStatus(string? stripeStatus) => stripeStatus switch
    {
        "active" => SubscriptionStatus.Active,
        "past_due" => SubscriptionStatus.PastDue,
        "canceled" => SubscriptionStatus.Cancelled,
        "trialing" => SubscriptionStatus.Trialing,
        _ => SubscriptionStatus.Active
    };
}
