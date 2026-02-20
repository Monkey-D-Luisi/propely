// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Email;

public record BaseEmailModel
{
    public string AppName { get; init; } = "SaaS Starter Kit";
    /// <summary>
    /// Support URL shown in email footers.
    /// TODO: Configure via ORGSAPI_App__SupportUrl env var; this placeholder must not reach production.
    /// </summary>
    public string SupportUrl { get; init; } = "https://example.com/support";
    public int Year { get; init; } = DateTime.UtcNow.Year;
}
