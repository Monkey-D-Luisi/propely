// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Billing.Commands.CreateCheckoutSession;

public sealed record CreateCheckoutSessionCommand(
    Guid OrgId,
    Guid UserId,
    string PlanId,
    string SuccessUrl,
    string CancelUrl) : IRequest<CreateCheckoutSessionResult>;

public sealed record CreateCheckoutSessionResult(string CheckoutUrl);
