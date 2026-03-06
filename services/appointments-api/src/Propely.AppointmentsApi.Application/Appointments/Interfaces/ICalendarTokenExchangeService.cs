// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Application.Appointments.Interfaces;

public sealed record CalendarTokenResult(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    string CalendarId);

public interface ICalendarTokenExchangeService
{
    Task<CalendarTokenResult> ExchangeCodeAsync(CalendarProvider provider, string authCode, string redirectUri, CancellationToken ct = default);
}
