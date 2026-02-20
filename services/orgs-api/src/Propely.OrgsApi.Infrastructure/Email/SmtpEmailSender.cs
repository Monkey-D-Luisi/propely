// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Propely.OrgsApi.Application.Common.Interfaces;

namespace Propely.OrgsApi.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var from = _configuration["Smtp:From"] ?? "no-reply@propely.test";
        var host = _configuration["Smtp:Host"] ?? "localhost";
        var port = int.TryParse(_configuration["Smtp:Port"], out var configuredPort) ? configuredPort : 10025;
        var enableSsl = !bool.TryParse(_configuration["Smtp:EnableSsl"], out var configuredSsl) || configuredSsl;

        using var message = new MailMessage(from, to, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = CredentialCache.DefaultNetworkCredentials
        };

        await client.SendMailAsync(message, cancellationToken);
    }

    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var host = _configuration["Smtp:Host"] ?? "localhost";
        var port = int.TryParse(_configuration["Smtp:Port"], out var configuredPort) ? configuredPort : 10025;

        try
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port, cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SMTP health check failed for {Host}:{Port}", host, port);
            return false;
        }
    }
}
