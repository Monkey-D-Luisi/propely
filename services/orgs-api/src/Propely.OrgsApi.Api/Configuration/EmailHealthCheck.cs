// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Propely.OrgsApi.Application.Common.Interfaces;

namespace Propely.OrgsApi.Api.Configuration;

public sealed class EmailHealthCheck : IHealthCheck
{
    private readonly IEmailSender _emailSender;

    public EmailHealthCheck(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var healthy = await _emailSender.CheckHealthAsync(cancellationToken);
        return healthy
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Email provider unreachable.");
    }
}
