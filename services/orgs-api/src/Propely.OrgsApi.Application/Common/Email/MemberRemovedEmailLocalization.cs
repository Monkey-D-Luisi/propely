// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Email;

public static class MemberRemovedEmailLocalization
{
    public static string BuildSubject(string locale, string organizationName)
    {
        var normalizedLocale = InvitationEmailLocalization.NormalizeLocale(locale);
        return normalizedLocale == InvitationEmailLocalization.SpanishLocale
            ? $"Has sido eliminado de {organizationName}"
            : $"You have been removed from {organizationName}";
    }
}
