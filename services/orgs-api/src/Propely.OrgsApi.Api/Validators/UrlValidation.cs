// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Api.Validators;

public static class UrlValidation
{
    public static bool IsAllowedRelativePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (!path.StartsWith('/'))
            return false;

        if (path.StartsWith("//"))
            return false;

        if (path.Contains("://", StringComparison.Ordinal))
            return false;

        return true;
    }
}
