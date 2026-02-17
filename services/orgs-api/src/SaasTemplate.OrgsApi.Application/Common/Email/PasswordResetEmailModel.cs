// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.OrgsApi.Application.Common.Email;

public sealed record PasswordResetEmailModel : BaseEmailModel
{
    public string ResetUrl { get; init; } = string.Empty;
}
