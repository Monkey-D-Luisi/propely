// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;

namespace Propely.OrgsApi.Domain.Users;

public sealed class RefreshToken : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    private RefreshToken() { }

    public static (RefreshToken entity, string plainToken) Create(Guid userId, TimeSpan lifetime)
    {
        var plainToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        var tokenHash = HashToken(plainToken);
        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAtUtc = DateTime.UtcNow.Add(lifetime),
            CreatedAtUtc = DateTime.UtcNow,
            IsRevoked = false
        };
        return (entity, plainToken);
    }

    public void Revoke(Guid? replacedByTokenId = null)
    {
        IsRevoked = true;
        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }

    public bool IsActive => !IsRevoked && ExpiresAtUtc > DateTime.UtcNow;

    public static string HashToken(string plainToken)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(plainToken));
        return Convert.ToBase64String(bytes);
    }
}
