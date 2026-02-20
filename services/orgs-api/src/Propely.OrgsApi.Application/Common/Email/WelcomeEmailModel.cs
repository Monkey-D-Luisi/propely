// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Email;

public sealed record WelcomeEmailModel : BaseEmailModel
{
    public string UserName { get; init; } = string.Empty;
    public string LoginUrl { get; init; } = string.Empty;
}
