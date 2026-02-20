// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Email;

public sealed record LayoutEmailModel : BaseEmailModel
{
    public string BodyHtml { get; init; } = string.Empty;
    public string Locale { get; init; } = InvitationEmailLocalization.DefaultLocale;
    public string FooterHelpPrefix { get; init; } = "Need help? Visit";
}
