// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AppointmentsApi.Infrastructure.Calendar;

public sealed class CalendarConfiguration
{
    public const string SectionName = "Calendar";

    public string? GoogleClientId { get; set; }
    public string? GoogleClientSecret { get; set; }
    public string? MicrosoftClientId { get; set; }
    public string? MicrosoftClientSecret { get; set; }
    public string? MicrosoftTenantId { get; set; }
    public int SyncIntervalMinutes { get; set; } = 5;
}
