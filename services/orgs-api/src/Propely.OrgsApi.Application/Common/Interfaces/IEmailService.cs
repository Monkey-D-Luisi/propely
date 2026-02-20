// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendOrgInvitationEmailAsync(
        string toEmail,
        string orgName,
        string inviteUrl,
        CancellationToken cancellationToken = default,
        string locale = "en");

    Task SendEmailVerificationEmailAsync(
        string toEmail,
        string verificationUrl,
        CancellationToken cancellationToken = default,
        string locale = "en");

    Task SendPasswordResetEmailAsync(
        string toEmail,
        string resetUrl,
        CancellationToken cancellationToken = default,
        string locale = "en");

    Task SendWelcomeEmailAsync(
        string toEmail,
        string userName,
        string loginUrl,
        CancellationToken cancellationToken = default,
        string locale = "en");

    Task SendRoleChangeEmailAsync(
        string toEmail,
        string userName,
        string orgName,
        string oldRole,
        string newRole,
        CancellationToken cancellationToken = default,
        string locale = "en");

    Task SendMemberRemovedEmailAsync(
        string toEmail,
        string userName,
        string orgName,
        CancellationToken cancellationToken = default,
        string locale = "en");

    Task SendPaymentFailureEmailAsync(
        string toEmail,
        string orgName,
        CancellationToken cancellationToken = default,
        string locale = "en");

    Task SendAccountDeletionConfirmationEmailAsync(
        string toEmail,
        string userName,
        CancellationToken cancellationToken = default,
        string locale = "en");
}
