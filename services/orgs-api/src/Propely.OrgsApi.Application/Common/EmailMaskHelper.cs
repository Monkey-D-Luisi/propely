// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common;

/// <summary>
/// Masks an email address for safe logging (e.g., "t***@example.com").
/// </summary>
public static class EmailMaskHelper
{
    /// <summary>
    /// Returns a masked version of the email suitable for log output.
    /// </summary>
    public static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
            return "***";

        var visiblePrefix = email[..1];
        var domain = email.AsSpan(atIndex);
        return $"{visiblePrefix}***{domain}";
    }
}
