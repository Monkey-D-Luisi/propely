// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.OrgsApi.Application.Common.Email;

public sealed record WelcomeEmailModel : BaseEmailModel
{
    public string UserName { get; init; } = string.Empty;
    public string LoginUrl { get; init; } = string.Empty;
}
