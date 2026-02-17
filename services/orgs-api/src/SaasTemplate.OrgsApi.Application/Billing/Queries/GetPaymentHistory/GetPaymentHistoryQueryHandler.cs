// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Billing.Interfaces;
using SaasTemplate.OrgsApi.Application.Common.Models;
using SaasTemplate.OrgsApi.Application.Organizations.Interfaces;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;
using MediatR;

namespace SaasTemplate.OrgsApi.Application.Billing.Queries.GetPaymentHistory;

public sealed class GetPaymentHistoryQueryHandler : IRequestHandler<GetPaymentHistoryQuery, PagedResult<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMembershipRepository _membershipRepository;

    public GetPaymentHistoryQueryHandler(
        IPaymentRepository paymentRepository,
        IMembershipRepository membershipRepository)
    {
        _paymentRepository = paymentRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<PagedResult<PaymentDto>> Handle(GetPaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        var membership = await _membershipRepository.GetAsync(request.OrgId, request.UserId, cancellationToken);

        if (membership is null)
            throw new ForbiddenException("User is not a member of this organization.");

        var pagedPayments = await _paymentRepository.GetByOrgIdPagedAsync(
            request.OrgId, request.Page, request.PageSize, cancellationToken);

        var dtos = pagedPayments.Items
            .Select(p => new PaymentDto(
                p.Id,
                p.Amount,
                p.Currency,
                p.Description,
                p.Status.ToString().ToLowerInvariant(),
                p.CreatedAtUtc))
            .ToList();

        return new PagedResult<PaymentDto>(dtos, pagedPayments.TotalCount, pagedPayments.PageNumber, request.PageSize);
    }
}
