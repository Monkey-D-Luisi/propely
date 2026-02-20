// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Infrastructure.Messaging.Configuration;

/// <summary>
/// Configuration options for the Outbox Dispatcher service.
/// </summary>
public sealed class OutboxDispatcherConfiguration
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "OutboxDispatcher";

    /// <summary>
    /// Polling interval in seconds (default: 5).
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Maximum number of messages to process per batch (default: 100).
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Whether the dispatcher is enabled (default: true).
    /// </summary>
    public bool Enabled { get; set; } = true;
}
