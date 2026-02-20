// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Billing.Commands.CreateCustomerPortalSession;

public sealed record CreateCustomerPortalSessionCommand(
    Guid OrgId,
    Guid UserId,
    string ReturnUrl) : IRequest<CreateCustomerPortalSessionResult>;

public sealed record CreateCustomerPortalSessionResult(string PortalUrl);
