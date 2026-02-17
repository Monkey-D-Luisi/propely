// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.OrgsApi.Application.Common.Email;

public sealed record LayoutEmailModel : BaseEmailModel
{
    public string BodyHtml { get; init; } = string.Empty;
    public string Locale { get; init; } = InvitationEmailLocalization.DefaultLocale;
    public string FooterHelpPrefix { get; init; } = "Need help? Visit";
}
