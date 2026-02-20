// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Email;

public sealed record MemberRemovedEmailModel : BaseEmailModel
{
    public string UserName { get; init; } = string.Empty;
    public string OrganizationName { get; init; } = string.Empty;
}
