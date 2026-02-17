// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Billing.Interfaces;
using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Organizations.Interfaces;
using SaasTemplate.OrgsApi.Application.Users.Interfaces;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;
using SaasTemplate.OrgsApi.Domain.Organizations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SaasTemplate.OrgsApi.Application.Users.Commands.DeleteAccount;

public sealed class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaymentService _paymentService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAccountCommandHandler> _logger;

    public DeleteAccountCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IMembershipRepository membershipRepository,
        IOrganizationRepository organizationRepository,
        IInvitationRepository invitationRepository,
        ISubscriptionRepository subscriptionRepository,
        IPaymentService paymentService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<DeleteAccountCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _membershipRepository = membershipRepository;
        _organizationRepository = organizationRepository;
        _invitationRepository = invitationRepository;
        _subscriptionRepository = subscriptionRepository;
        _paymentService = paymentService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User {request.UserId} not found.");

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Security: Failed account deletion attempt for user {UserId}", request.UserId);
            throw new UnauthorizedAccessException("INVALID_PASSWORD");
        }

        // Get all memberships for this user
        var memberships = await _membershipRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        // Batch-load all org memberships in a single query (avoids N+1)
        var orgIds = memberships.Select(m => m.OrganizationId).Distinct().ToList();
        var allOrgMembers = await _membershipRepository.GetByOrgIdsAsync(orgIds, cancellationToken);
        var orgMembersByOrgId = allOrgMembers
            .GroupBy(m => m.OrganizationId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Identify orgs where user is sole owner
        var soleOwnerOrgIds = new List<Guid>();
        foreach (var membership in memberships)
        {
            var orgMembers = orgMembersByOrgId.GetValueOrDefault(membership.OrganizationId, []);
            var ownerCount = orgMembers.Count(m => m.Role == MembershipRole.Owner);
            var isSoleOwner = ownerCount == 1 && orgMembers.Any(m => m.Role == MembershipRole.Owner && m.UserId == request.UserId);

            if (isSoleOwner)
            {
                soleOwnerOrgIds.Add(membership.OrganizationId);
            }
            else
            {
                // Not sole owner: just remove user's membership
                membership.SoftDelete();
            }
        }

        // Batch-load organizations for sole-owner orgs in a single query
        if (soleOwnerOrgIds.Count > 0)
        {
            var orgs = await _organizationRepository.GetByIdsAsync(soleOwnerOrgIds, cancellationToken);

            foreach (var org in orgs)
            {
                org.SoftDelete();

                // Cancel Stripe subscription if billing is enabled
                await CancelOrgSubscriptionAsync(org.Id, cancellationToken);

                // Soft-delete all memberships in this org
                var orgMembers = orgMembersByOrgId.GetValueOrDefault(org.Id, []);
                foreach (var member in orgMembers)
                {
                    member.SoftDelete();
                }

                // Cancel pending invitations
                await _invitationRepository.CancelByOrgAsync(org.Id, cancellationToken);
            }
        }

        // Soft-delete the user
        user.SoftDelete();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Security: Account deleted for user {UserId}",
            request.UserId);

        // Send confirmation email (best-effort, after commit)
        await _emailService.SendAccountDeletionConfirmationEmailAsync(
            user.Email,
            user.Name ?? user.Email,
            cancellationToken);
    }

    private async Task CancelOrgSubscriptionAsync(Guid orgId, CancellationToken cancellationToken)
    {
        if (!_paymentService.IsEnabled) return;

        var subscription = await _subscriptionRepository.GetByOrgIdAsync(orgId, cancellationToken);
        if (subscription is null) return;

        try
        {
            await _paymentService.CancelSubscriptionAsync(
                subscription.StripeSubscriptionId, cancellationToken);

            subscription.Cancel();

            _logger.LogInformation(
                "Cancelled subscription {SubscriptionId} for org {OrgId} during account deletion",
                subscription.StripeSubscriptionId,
                orgId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to cancel subscription {SubscriptionId} for org {OrgId} during account deletion. " +
                "The subscription may need manual cancellation in Stripe dashboard.",
                subscription.StripeSubscriptionId,
                orgId);
        }
    }
}
