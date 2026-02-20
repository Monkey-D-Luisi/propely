// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Api.Configuration;

public sealed class VersionOptions
{
    public string Current { get; set; } = "0.0.0";
    public string? BuildDate { get; set; }
    public string? GitHubRepo { get; set; }
}
