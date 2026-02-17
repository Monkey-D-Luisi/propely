// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Api.Dtos;
using SaasTemplate.OrgsApi.Api.Extensions;
using SaasTemplate.OrgsApi.Application.Billing.Commands.CreateCheckoutSession;
using SaasTemplate.OrgsApi.Application.Billing.Commands.CreateCustomerPortalSession;
using SaasTemplate.OrgsApi.Application.Billing.Commands.CreatePaymentSession;
using SaasTemplate.OrgsApi.Application.Billing.Commands.ProcessWebhookEvent;
using SaasTemplate.OrgsApi.Application.Billing.Queries.GetPlans;
using SaasTemplate.OrgsApi.Application.Billing.Queries.GetSubscription;
using SaasTemplate.OrgsApi.Application.Billing.Queries.GetPaymentHistory;
using SaasTemplate.OrgsApi.Api.Services;
using SaasTemplate.OrgsApi.Application.Billing;
using SaasTemplate.OrgsApi.Application.Billing.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace SaasTemplate.OrgsApi.Api.Controllers;

[ApiController]
[Route("billing")]
[Authorize]
public sealed class BillingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CheckoutRequest> _checkoutValidator;
    private readonly IValidator<CustomerPortalRequest> _portalValidator;
    private readonly IValidator<PaymentRequest> _paymentValidator;
    private readonly string _frontendBaseUrl;
    private readonly string _webhookSecret;
    private readonly IReadOnlyDictionary<string, string> _priceIdToPlanId;
    private readonly IWebhookEventMapper _webhookEventMapper;
    private readonly ILogger<BillingController> _logger;

    public BillingController(
        IMediator mediator,
        IValidator<CheckoutRequest> checkoutValidator,
        IValidator<CustomerPortalRequest> portalValidator,
        IValidator<PaymentRequest> paymentValidator,
        IConfiguration configuration,
        IOptions<BillingConfiguration> billingConfig,
        IWebhookEventMapper webhookEventMapper,
        ILogger<BillingController> logger)
    {
        _mediator = mediator;
        _checkoutValidator = checkoutValidator;
        _portalValidator = portalValidator;
        _paymentValidator = paymentValidator;
        _frontendBaseUrl = (configuration["Auth:FrontendBaseUrl"] ?? "http://localhost:3000").TrimEnd('/');
        _webhookSecret = billingConfig.Value.Stripe.WebhookSecret;
        _priceIdToPlanId = billingConfig.Value.Plans
            .Where(p => !string.IsNullOrEmpty(p.StripePriceId))
            .GroupBy(p => p.StripePriceId)
            .ToDictionary(g => g.Key, g => g.First().Id);
        _webhookEventMapper = webhookEventMapper;
        _logger = logger;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckout([FromBody] CheckoutRequest request, CancellationToken cancellationToken)
    {
        if (!CsrfValidator.Validate(Request))
            return StatusCode(403, new { error = "Invalid CSRF token." });

        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var validationResult = await _checkoutValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem();
        }

        var command = new CreateCheckoutSessionCommand(
            request.OrgId,
            userId.Value,
            request.PlanId,
            _frontendBaseUrl + request.SuccessUrl,
            _frontendBaseUrl + request.CancelUrl);

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(new { checkoutUrl = result.CheckoutUrl });
    }

    [HttpGet("subscription")]
    public async Task<IActionResult> GetSubscription([FromQuery] Guid orgId, CancellationToken cancellationToken)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        if (orgId == Guid.Empty)
            return BadRequest(new { error = "Organization ID is required." });

        var query = new GetSubscriptionQuery(orgId, userId.Value);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlans(CancellationToken cancellationToken)
    {
        var plans = await _mediator.Send(new GetPlansQuery(), cancellationToken);
        return Ok(plans);
    }

    [HttpPost("customer-portal")]
    public async Task<IActionResult> CreateCustomerPortal(
        [FromBody] CustomerPortalRequest request,
        CancellationToken cancellationToken)
    {
        if (!CsrfValidator.Validate(Request))
            return StatusCode(403, new { error = "Invalid CSRF token." });

        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var validationResult = await _portalValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem();
        }

        var command = new CreateCustomerPortalSessionCommand(
            request.OrgId,
            userId.Value,
            _frontendBaseUrl + request.ReturnUrl);

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(new { portalUrl = result.PortalUrl });
    }

    [HttpPost("payment")]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request, CancellationToken cancellationToken)
    {
        if (!CsrfValidator.Validate(Request))
            return StatusCode(403, new { error = "Invalid CSRF token." });

        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        var validationResult = await _paymentValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem();
        }

        var command = new CreatePaymentSessionCommand(
            request.OrgId,
            userId.Value,
            request.Amount,
            request.Currency,
            request.Description,
            _frontendBaseUrl + request.SuccessUrl,
            _frontendBaseUrl + request.CancelUrl);

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(new { checkoutUrl = result.CheckoutUrl });
    }

    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments(
        [FromQuery] Guid orgId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = this.GetUserId();
        if (userId is null) return Unauthorized();

        if (orgId == Guid.Empty)
            return BadRequest(new { error = "Organization ID is required." });

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = new GetPaymentHistoryQuery(orgId, userId.Value, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    [RequestSizeLimit(65_536)]
    public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(HttpContext.Request.Body);
        var json = await reader.ReadToEndAsync(cancellationToken);
        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _webhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning("Webhook signature verification failed: {Message}", ex.Message);
            return BadRequest();
        }

        var eventData = _webhookEventMapper.MapToEventData(stripeEvent, _priceIdToPlanId);

        await _mediator.Send(
            new ProcessWebhookEventCommand(stripeEvent.Id, stripeEvent.Type, eventData),
            cancellationToken);

        return Ok();
    }
}
