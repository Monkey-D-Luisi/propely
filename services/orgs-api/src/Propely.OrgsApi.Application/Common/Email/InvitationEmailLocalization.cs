// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Email;

public static class InvitationEmailLocalization
{
    public const string DefaultLocale = "en";
    public const string SpanishLocale = "es";

    public static string NormalizeLocale(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return DefaultLocale;
        }

        return locale.Trim().StartsWith(SpanishLocale, StringComparison.OrdinalIgnoreCase)
            ? SpanishLocale
            : DefaultLocale;
    }

    public static string ResolveLocale(string? requestedLocale, string? fallbackLocale)
    {
        if (!string.IsNullOrWhiteSpace(requestedLocale))
        {
            return NormalizeLocale(requestedLocale);
        }

        if (!string.IsNullOrWhiteSpace(fallbackLocale))
        {
            return NormalizeLocale(fallbackLocale);
        }

        return DefaultLocale;
    }

    public static string BuildInvitationSubject(string locale, string organizationName)
    {
        var normalizedLocale = NormalizeLocale(locale);
        return normalizedLocale == SpanishLocale
            ? $"Invitaci\u00F3n para unirte a {organizationName}"
            : $"Invitation to join {organizationName}";
    }

    public static string BuildFooterHelpPrefix(string locale)
    {
        var normalizedLocale = NormalizeLocale(locale);
        return normalizedLocale == SpanishLocale
            ? "\u00BFNecesitas ayuda? Visita"
            : "Need help? Visit";
    }
}
