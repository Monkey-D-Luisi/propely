// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.PropertiesApi.Infrastructure.Messaging.Configuration;

/// <summary>
/// Configuration options for RabbitMQ connection and messaging.
/// </summary>
public sealed class RabbitMqConfiguration
{
    public const string SectionName = "RabbitMQ";

    private string _host = "localhost";
    public string Host { get => _host; set => _host = value?.Trim() ?? "localhost"; }

    public int Port { get; set; } = 5672;

    private string _username = "guest";
    public string Username { get => _username; set => _username = value?.Trim() ?? "guest"; }

    private string _password = "guest";
    public string Password { get => _password; set => _password = value?.Trim() ?? ""; }

    private string _virtualHost = "/";
    public string VirtualHost { get => _virtualHost; set => _virtualHost = value?.Trim() ?? "/"; }

    /// <summary>
    /// The exchange name for domain events.
    /// </summary>
    public string EventsExchange { get; set; } = "properties.events";

    public bool UseSsl { get; set; }

    public int ConnectionTimeoutSeconds { get; set; } = 30;
}
