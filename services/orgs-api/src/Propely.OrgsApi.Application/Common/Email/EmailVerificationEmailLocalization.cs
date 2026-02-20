// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Email;

public static class EmailVerificationEmailLocalization
{
    public static string BuildSubject(string locale)
    {
        var normalizedLocale = InvitationEmailLocalization.NormalizeLocale(locale);
        return normalizedLocale == InvitationEmailLocalization.SpanishLocale
            ? "Verifica tu correo electr\u00F3nico"
            : "Verify your email address";
    }
}
