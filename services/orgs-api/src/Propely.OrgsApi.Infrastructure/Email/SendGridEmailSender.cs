// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Propely.OrgsApi.Application.Common.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Propely.OrgsApi.Infrastructure.Email;

public sealed class SendGridEmailSender : IEmailSender
{
    private readonly ISendGridClient _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendGridEmailSender> _logger;

    public SendGridEmailSender(
        ISendGridClient client,
        IConfiguration configuration,
        ILogger<SendGridEmailSender> logger)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var fromAddress = _configuration["SendGrid:From"] ?? _configuration["Smtp:From"] ?? "no-reply@propely.test";
        var fromName = _configuration["SendGrid:FromName"] ?? "Propely";

        var msg = new SendGridMessage
        {
            From = new EmailAddress(fromAddress, fromName),
            Subject = subject,
            HtmlContent = htmlBody
        };
        msg.AddTo(new EmailAddress(to));

        var response = await _client.SendEmailAsync(msg, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "SendGrid API returned {StatusCode} when sending email.",
                (int)response.StatusCode);
            throw new InvalidOperationException(
                $"SendGrid API returned {(int)response.StatusCode}.");
        }
    }

    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // A lightweight call to verify API key validity.
            var response = await _client.RequestAsync(
                method: SendGridClient.Method.GET,
                urlPath: "scopes",
                cancellationToken: cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SendGrid health check failed");
            return false;
        }
    }
}
