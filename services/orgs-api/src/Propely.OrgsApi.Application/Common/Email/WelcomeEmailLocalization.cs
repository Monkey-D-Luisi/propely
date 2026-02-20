// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Email;

public static class WelcomeEmailLocalization
{
    public static string BuildSubject(string locale, string appName)
    {
        var normalizedLocale = InvitationEmailLocalization.NormalizeLocale(locale);
        return normalizedLocale == InvitationEmailLocalization.SpanishLocale
            ? $"Bienvenido a {appName}"
            : $"Welcome to {appName}";
    }
}
