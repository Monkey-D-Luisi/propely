// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Infrastructure.Messaging.Configuration;

/// <summary>
/// Configuration options for RabbitMQ connection and messaging.
/// </summary>
public sealed class RabbitMqConfiguration
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "RabbitMQ";

    /// <summary>
    /// RabbitMQ host name (default: localhost).
    /// Trimmed on set to guard against trailing whitespace from Secret Manager.
    /// </summary>
    private string _host = "localhost";
    public string Host { get => _host; set => _host = value?.Trim() ?? "localhost"; }

    /// <summary>
    /// RabbitMQ port (default: 5672).
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// RabbitMQ username.
    /// Trimmed on set to guard against trailing whitespace from Secret Manager.
    /// </summary>
    private string _username = "guest";
    public string Username { get => _username; set => _username = value?.Trim() ?? "guest"; }

    /// <summary>
    /// RabbitMQ password.
    /// Trimmed on set to guard against trailing whitespace from Secret Manager.
    /// </summary>
    private string _password = "guest";
    public string Password { get => _password; set => _password = value?.Trim() ?? ""; }

    /// <summary>
    /// RabbitMQ virtual host (default: /).
    /// Trimmed on set to guard against trailing whitespace from Secret Manager.
    /// </summary>
    private string _virtualHost = "/";
    public string VirtualHost { get => _virtualHost; set => _virtualHost = value?.Trim() ?? "/"; }

    /// <summary>
    /// The exchange name for work item events.
    /// </summary>
    public string WorkItemsExchange { get; set; } = "workitems.events";

    /// <summary>
    /// Whether to use SSL/TLS for the connection (default: false).
    /// Required for cloud-hosted brokers such as CloudAMQP.
    /// </summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// Connection timeout in seconds (default: 30).
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;
}
