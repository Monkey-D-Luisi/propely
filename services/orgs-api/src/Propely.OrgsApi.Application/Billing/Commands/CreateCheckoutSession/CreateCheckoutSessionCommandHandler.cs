// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using MediatR;

namespace Propely.OrgsApi.Application.Billing.Commands.CreateCheckoutSession;

public sealed class CreateCheckoutSessionCommandHandler : IRequestHandler<CreateCheckoutSessionCommand, CreateCheckoutSessionResult>
{
    private readonly IPaymentService _paymentService;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IPlanProvider _planProvider;

    public CreateCheckoutSessionCommandHandler(
        IPaymentService paymentService,
        IOrganizationRepository organizationRepository,
        IMembershipRepository membershipRepository,
        IPlanProvider planProvider)
    {
        _paymentService = paymentService;
        _organizationRepository = organizationRepository;
        _membershipRepository = membershipRepository;
        _planProvider = planProvider;
    }

    public async Task<CreateCheckoutSessionResult> Handle(CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
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

        if (_planProvider.GetPlan(request.PlanId) is null)
            throw new DomainException($"Unknown plan '{request.PlanId}'.");

        var checkoutUrl = await _paymentService.CreateCheckoutSessionAsync(
            request.OrgId,
            request.PlanId,
            request.SuccessUrl,
            request.CancelUrl,
            cancellationToken);

        return new CreateCheckoutSessionResult(checkoutUrl);
    }
}
