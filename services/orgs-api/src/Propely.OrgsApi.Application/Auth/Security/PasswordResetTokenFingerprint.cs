// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Cryptography;
using System.Text;

namespace Propely.OrgsApi.Application.Auth.Security;

public static class PasswordResetTokenFingerprint
{
    public static string FromPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(passwordHash));
        return Convert.ToHexString(hashBytes);
    }
}
