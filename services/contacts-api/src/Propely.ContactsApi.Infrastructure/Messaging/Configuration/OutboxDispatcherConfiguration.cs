// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.ContactsApi.Infrastructure.Messaging.Configuration;

/// <summary>
/// Configuration options for the Outbox Dispatcher service.
/// </summary>
public sealed class OutboxDispatcherConfiguration
{
    public const string SectionName = "OutboxDispatcher";

    public int PollingIntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 100;
    public bool Enabled { get; set; } = true;
}
