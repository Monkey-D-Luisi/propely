// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Models;
using MediatR;

namespace Propely.OrgsApi.Application.Billing.Queries.GetPaymentHistory;

public sealed record GetPaymentHistoryQuery(Guid OrgId, Guid UserId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<PaymentDto>>;

public sealed record PaymentDto(
    Guid Id,
    long Amount,
    string Currency,
    string Description,
    string Status,
    DateTime CreatedAtUtc);
