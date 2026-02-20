// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Infrastructure.Messaging.Configuration;

/// <summary>
/// Configuration options for the WorkItem projector service.
/// </summary>
public sealed class ProjectorConfiguration
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Projector";

    /// <summary>
    /// Whether the projector is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The queue name for receiving work item events.
    /// </summary>
    public string QueueName { get; set; } = "projector.workitems";

    /// <summary>
    /// The dead-letter exchange name for failed messages.
    /// </summary>
    public string DeadLetterExchange { get; set; } = "workitems.events.dlx";

    /// <summary>
    /// The dead-letter queue name.
    /// </summary>
    public string DeadLetterQueue { get; set; } = "projector.workitems.dlq";

    /// <summary>
    /// Prefetch count for the consumer (default: 10).
    /// </summary>
    public ushort PrefetchCount { get; set; } = 10;
}
