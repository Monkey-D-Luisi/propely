// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AppointmentsApi.Domain.Appointments;

public sealed class CalendarSyncInfo
{
    public string ExternalEventId { get; private set; } = null!;
    public CalendarProvider Provider { get; private set; }
    public DateTime LastSyncedUtc { get; private set; }

    private CalendarSyncInfo() { }

    public CalendarSyncInfo(string externalEventId, CalendarProvider provider, DateTime lastSyncedUtc)
    {
        if (string.IsNullOrWhiteSpace(externalEventId))
            throw new ArgumentException("External event ID is required.", nameof(externalEventId));

        ExternalEventId = externalEventId;
        Provider = provider;
        LastSyncedUtc = lastSyncedUtc;
    }

    public void UpdateLastSynced(DateTime syncedAtUtc)
    {
        LastSyncedUtc = syncedAtUtc;
    }
}
