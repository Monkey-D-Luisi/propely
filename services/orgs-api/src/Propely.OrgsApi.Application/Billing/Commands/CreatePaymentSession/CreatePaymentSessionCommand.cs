// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Billing.Commands.CreatePaymentSession;

public sealed record CreatePaymentSessionCommand(
    Guid OrgId,
    Guid UserId,
    long Amount,
    string Currency,
    string Description,
    string SuccessUrl,
    string CancelUrl) : IRequest<CreatePaymentSessionResult>;

public sealed record CreatePaymentSessionResult(string CheckoutUrl);
