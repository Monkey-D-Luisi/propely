// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.OrgsApi.Application.Common.Email;

public static class RoleChangeEmailLocalization
{
    public static string BuildSubject(string locale, string organizationName)
    {
        var normalizedLocale = InvitationEmailLocalization.NormalizeLocale(locale);
        return normalizedLocale == InvitationEmailLocalization.SpanishLocale
            ? $"Tu rol en {organizationName} ha cambiado"
            : $"Your role in {organizationName} has changed";
    }
}
