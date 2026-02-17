// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Common.Models;
using SaasTemplate.OrgsApi.Domain.Billing;

namespace SaasTemplate.OrgsApi.Application.Billing.Interfaces;

public interface IPaymentRepository
{
    Task<List<Payment>> GetByOrgIdAsync(Guid orgId, CancellationToken cancellationToken = default);
    Task<PagedResult<Payment>> GetByOrgIdPagedAsync(Guid orgId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Payment?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId, CancellationToken cancellationToken = default);
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}
