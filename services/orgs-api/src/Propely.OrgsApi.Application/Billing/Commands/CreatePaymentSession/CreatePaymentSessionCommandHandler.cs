// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using MediatR;

namespace Propely.OrgsApi.Application.Billing.Commands.CreatePaymentSession;

public sealed class CreatePaymentSessionCommandHandler : IRequestHandler<CreatePaymentSessionCommand, CreatePaymentSessionResult>
{
    private readonly IPaymentService _paymentService;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;

    public CreatePaymentSessionCommandHandler(
        IPaymentService paymentService,
        IOrganizationRepository organizationRepository,
        IMembershipRepository membershipRepository)
    {
        _paymentService = paymentService;
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<CreatePaymentSessionResult> Handle(CreatePaymentSessionCommand request, CancellationToken cancellationToken)
    {
        _ = await _organizationRepository.GetByIdAsync(request.OrgId, cancellationToken)
            ?? throw new NotFoundException($"Organization {request.OrgId} not found.");

        var membership = await _membershipRepository.GetAsync(request.OrgId, request.UserId, cancellationToken);

        if (membership is null)
            throw new ForbiddenException("User is not a member of this organization.");

        if (membership.Role is not (MembershipRole.Owner or MembershipRole.Admin))
            throw new ForbiddenException("Only owners and admins can manage billing.");

        if (!_paymentService.IsEnabled)
            throw new DomainException("Billing is not enabled. Set Billing:Mode to \"stripe\" to enable checkout.");

        var checkoutUrl = await _paymentService.CreatePaymentCheckoutSessionAsync(
            request.OrgId,
            request.Amount,
            request.Currency,
            request.Description,
            request.SuccessUrl,
            request.CancelUrl,
            cancellationToken);

        return new CreatePaymentSessionResult(checkoutUrl);
    }
}
