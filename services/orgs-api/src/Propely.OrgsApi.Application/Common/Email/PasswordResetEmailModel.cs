// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Email;

public sealed record PasswordResetEmailModel : BaseEmailModel
{
    public string ResetUrl { get; init; } = string.Empty;
}
