// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.OrgsApi.Application.Common.Email;

public sealed record RoleChangeEmailModel : BaseEmailModel
{
    public string UserName { get; init; } = string.Empty;
    public string OrganizationName { get; init; } = string.Empty;
    public string OldRole { get; init; } = string.Empty;
    public string NewRole { get; init; } = string.Empty;
}
