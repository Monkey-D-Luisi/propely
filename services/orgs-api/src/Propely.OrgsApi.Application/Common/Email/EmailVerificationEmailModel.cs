// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Email;

public sealed record EmailVerificationEmailModel : BaseEmailModel
{
    public string VerificationUrl { get; init; } = string.Empty;
}
