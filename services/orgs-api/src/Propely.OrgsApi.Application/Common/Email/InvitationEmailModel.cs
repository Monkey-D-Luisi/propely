// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Email;

public sealed record InvitationEmailModel : BaseEmailModel
{
    public string OrganizationName { get; init; } = string.Empty;
    public string InviteUrl { get; init; } = string.Empty;
}
