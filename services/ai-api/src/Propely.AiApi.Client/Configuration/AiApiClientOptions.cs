// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Client.Configuration;

/// <summary>
/// Configuration options for the AI API SDK client.
/// </summary>
public sealed class AiApiClientOptions
{
    /// <summary>
    /// Base URL of the AI API service.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:5010";

    /// <summary>
    /// HTTP request timeout. Defaults to 60 seconds because AI calls can be long-running.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
}
