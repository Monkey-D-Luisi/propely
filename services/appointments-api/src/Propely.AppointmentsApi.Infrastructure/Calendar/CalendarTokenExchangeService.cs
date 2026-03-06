// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Logging;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Infrastructure.Calendar;

public sealed class CalendarTokenExchangeService : ICalendarTokenExchangeService
{
    private readonly ILogger<CalendarTokenExchangeService> _logger;

    public CalendarTokenExchangeService(ILogger<CalendarTokenExchangeService> logger)
    {
        _logger = logger;
    }

    public Task<CalendarTokenResult> ExchangeCodeAsync(
        CalendarProvider provider,
        string authCode,
        string redirectUri,
        CancellationToken ct = default)
    {
        _logger.LogWarning(
            "CalendarTokenExchangeService.ExchangeCodeAsync called for provider {Provider} -- " +
            "this is a stub implementation. Real OAuth integration requires API keys.",
            provider);

        throw new NotImplementedException(
            $"OAuth token exchange for {provider} is not yet implemented. " +
            "Configure Calendar:GoogleClientId/GoogleClientSecret or Calendar:MicrosoftClientId/MicrosoftClientSecret " +
            "and implement the real OAuth flow.");
    }
}
