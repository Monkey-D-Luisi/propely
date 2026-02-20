// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Email;

public static class PasswordResetEmailLocalization
{
    public static string BuildPasswordResetSubject(string locale)
    {
        var normalizedLocale = InvitationEmailLocalization.NormalizeLocale(locale);
        return normalizedLocale == InvitationEmailLocalization.SpanishLocale
            ? "Restablece tu contrase\u00F1a"
            : "Reset your password";
    }
}
