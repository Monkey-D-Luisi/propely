// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Propely.OrgsApi.Application.Common.Email;
using Propely.OrgsApi.Application.Common.Interfaces;

namespace Propely.OrgsApi.Infrastructure.Services;

public sealed class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly IEmailTemplateRenderer _emailTemplateRenderer;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailService> _logger;
    private readonly string _appName;
    private readonly string _supportUrl;

    public EmailService(
        IConfiguration configuration,
        IEmailTemplateRenderer emailTemplateRenderer,
        IEmailSender emailSender,
        ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _emailTemplateRenderer = emailTemplateRenderer;
        _emailSender = emailSender;
        _logger = logger;

        _appName = _configuration["App:Name"] ?? "Propely";
        // TODO: Remove example.com fallback once App:SupportUrl is enforced in production config
        _supportUrl = _configuration["App:SupportUrl"] ?? "https://example.com/support";

        if (_appName == "Propely")
        {
            _logger.LogWarning("App:Name not configured, using default placeholder. Set ORGSAPI_App__Name environment variable.");
        }

        if (_supportUrl == "https://example.com/support")
        {
            _logger.LogWarning("App:SupportUrl not configured, using default placeholder. Set ORGSAPI_App__SupportUrl environment variable.");
        }
    }

    public Task SendOrgInvitationEmailAsync(
        string toEmail,
        string orgName,
        string inviteUrl,
        CancellationToken cancellationToken = default,
        string locale = "en")
    {
        var resolvedLocale = ResolveLocale(locale);
        var subject = InvitationEmailLocalization.BuildInvitationSubject(resolvedLocale, orgName);
        var templateModel = new InvitationEmailModel
        {
            AppName = _appName,
            SupportUrl = _supportUrl,
            OrganizationName = orgName,
            InviteUrl = inviteUrl
        };

        return SendTemplateEmailAsync(
            toEmail,
            "Invitation",
            templateModel,
            subject,
            resolvedLocale,
            "Invitation email sent for org {OrgName} with locale {Locale}",
            new object[] { orgName, resolvedLocale },
            cancellationToken,
            isCritical: true);
    }

    public Task SendEmailVerificationEmailAsync(
        string toEmail,
        string verificationUrl,
        CancellationToken cancellationToken = default,
        string locale = "en")
    {
        var resolvedLocale = ResolveLocale(locale);
        var subject = EmailVerificationEmailLocalization.BuildSubject(resolvedLocale);
        var templateModel = new EmailVerificationEmailModel
        {
            AppName = _appName,
            SupportUrl = _supportUrl,
            VerificationUrl = verificationUrl
        };

        return SendTemplateEmailAsync(
            toEmail,
            "EmailVerification",
            templateModel,
            subject,
            resolvedLocale,
            "Email verification message sent with locale {Locale}",
            new object[] { resolvedLocale },
            cancellationToken,
            isCritical: true);
    }

    public Task SendPasswordResetEmailAsync(
        string toEmail,
        string resetUrl,
        CancellationToken cancellationToken = default,
        string locale = "en")
    {
        var resolvedLocale = ResolveLocale(locale);
        var subject = PasswordResetEmailLocalization.BuildPasswordResetSubject(resolvedLocale);
        var templateModel = new PasswordResetEmailModel
        {
            AppName = _appName,
            SupportUrl = _supportUrl,
            ResetUrl = resetUrl
        };

        return SendTemplateEmailAsync(
            toEmail,
            "PasswordReset",
            templateModel,
            subject,
            resolvedLocale,
            "Password reset email sent with locale {Locale}",
            new object[] { resolvedLocale },
            cancellationToken,
            isCritical: true);
    }

    public Task SendWelcomeEmailAsync(
        string toEmail,
        string userName,
        string loginUrl,
        CancellationToken cancellationToken = default,
        string locale = "en")
    {
        var resolvedLocale = ResolveLocale(locale);
        var subject = WelcomeEmailLocalization.BuildSubject(resolvedLocale, _appName);
        var templateModel = new WelcomeEmailModel
        {
            AppName = _appName,
            SupportUrl = _supportUrl,
            UserName = userName,
            LoginUrl = loginUrl
        };

        return SendTemplateEmailAsync(
            toEmail,
            "Welcome",
            templateModel,
            subject,
            resolvedLocale,
            "Welcome email sent with locale {Locale}",
            new object[] { resolvedLocale },
            cancellationToken);
    }

    public Task SendRoleChangeEmailAsync(
        string toEmail,
        string userName,
        string orgName,
        string oldRole,
        string newRole,
        CancellationToken cancellationToken = default,
        string locale = "en")
    {
        var resolvedLocale = ResolveLocale(locale);
        var subject = RoleChangeEmailLocalization.BuildSubject(resolvedLocale, orgName);
        var templateModel = new RoleChangeEmailModel
        {
            AppName = _appName,
            SupportUrl = _supportUrl,
            UserName = userName,
            OrganizationName = orgName,
            OldRole = oldRole,
            NewRole = newRole
        };

        return SendTemplateEmailAsync(
            toEmail,
            "RoleChange",
            templateModel,
            subject,
            resolvedLocale,
            "Role change email sent for org {OrgName} with locale {Locale}",
            new object[] { orgName, resolvedLocale },
            cancellationToken);
    }

    public Task SendMemberRemovedEmailAsync(
        string toEmail,
        string userName,
        string orgName,
        CancellationToken cancellationToken = default,
        string locale = "en")
    {
        var resolvedLocale = ResolveLocale(locale);
        var subject = MemberRemovedEmailLocalization.BuildSubject(resolvedLocale, orgName);
        var templateModel = new MemberRemovedEmailModel
        {
            AppName = _appName,
            SupportUrl = _supportUrl,
            UserName = userName,
            OrganizationName = orgName
        };

        return SendTemplateEmailAsync(
            toEmail,
            "MemberRemoved",
            templateModel,
            subject,
            resolvedLocale,
            "Member removed email sent for org {OrgName} with locale {Locale}",
            new object[] { orgName, resolvedLocale },
            cancellationToken);
    }

    public async Task SendPaymentFailureEmailAsync(
        string toEmail,
        string orgName,
        CancellationToken cancellationToken = default,
        string locale = "en")
    {
        var encodedOrgName = System.Net.WebUtility.HtmlEncode(orgName);
        var subject = $"[{_appName}] Payment failed for {orgName}";
        var body = $"<p>A payment for your organization <strong>{encodedOrgName}</strong> has failed.</p>" +
                   $"<p>Please update your payment method to avoid service interruption.</p>" +
                   $"<p>If you need assistance, contact us at <a href=\"{_supportUrl}\">{_supportUrl}</a>.</p>";

        try
        {
            await _emailSender.SendAsync(toEmail, subject, body, cancellationToken);
            _logger.LogDebug("Payment failure email sent for org {OrgName}", orgName);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send payment failure email for org {OrgName}", orgName);
        }
    }

    public async Task SendAccountDeletionConfirmationEmailAsync(
        string toEmail,
        string userName,
        CancellationToken cancellationToken = default,
        string locale = "en")
    {
        var encodedName = System.Net.WebUtility.HtmlEncode(userName);
        var subject = $"[{_appName}] Your account has been deactivated";
        var body = $"<p>Hi {encodedName},</p>" +
                   $"<p>Your account has been deactivated and scheduled for deletion from <strong>{System.Net.WebUtility.HtmlEncode(_appName)}</strong> in accordance with our data retention policies.</p>" +
                   "<p>Certain data may be retained for a limited period as required for legal, regulatory, or security purposes.</p>" +
                   "<p>If you did not request this action, please contact support immediately.</p>" +
                   $"<p>If you need assistance, contact us at <a href=\"{_supportUrl}\">{_supportUrl}</a>.</p>";

        try
        {
            await _emailSender.SendAsync(toEmail, subject, body, cancellationToken);
            _logger.LogDebug("Account deletion confirmation email sent");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send account deletion confirmation email");
        }
    }

    private string ResolveLocale(string locale)
    {
        return InvitationEmailLocalization.ResolveLocale(
            locale,
            _configuration["Email:DefaultLocale"]);
    }

    private async Task SendTemplateEmailAsync<TModel>(
        string toEmail,
        string templateName,
        TModel model,
        string subject,
        string resolvedLocale,
        string successLogMessage,
        object[] successLogArgs,
        CancellationToken cancellationToken,
        bool isCritical = false)
        where TModel : BaseEmailModel
    {
        try
        {
            var body = await _emailTemplateRenderer.RenderAsync(
                templateName,
                model,
                resolvedLocale,
                cancellationToken);

            await _emailSender.SendAsync(toEmail, subject, body, cancellationToken);
            _logger.LogDebug(successLogMessage, successLogArgs);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            if (isCritical)
            {
                _logger.LogError(
                    ex,
                    "Failed to send critical {TemplateName} email to {ToEmail}",
                    templateName,
                    toEmail);
                throw;
            }

            _logger.LogWarning(
                ex,
                "Failed to send {TemplateName} email",
                templateName);
        }
    }
}
