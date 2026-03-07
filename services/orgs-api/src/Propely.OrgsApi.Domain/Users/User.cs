// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;

namespace Propely.OrgsApi.Domain.Users;

public sealed class User : Entity, ISoftDeletable
{
    public const int EmailMaxLength = 256;
    public const int NameMaxLength = 200;
    public const int MaxFailedLoginAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string? Name { get; private set; }
    public bool EmailVerified { get; private set; }
    public DateTime? EmailVerifiedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public int PasswordVersion { get; private set; }
    public bool IsSystemAdmin { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEndUtc { get; private set; }

    private User() { }

    public static User Create(string email, string passwordHash, string? name = null, bool emailVerified = false)
    {
        var now = DateTime.UtcNow;

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim(),
            EmailVerified = emailVerified,
            EmailVerifiedAtUtc = emailVerified ? now : null,
            IsDeleted = false,
            CreatedAtUtc = now
        };
    }

    public void MarkEmailAsVerified()
    {
        if (EmailVerified)
        {
            return;
        }

        var now = DateTime.UtcNow;
        EmailVerified = true;
        EmailVerifiedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void UpdateProfile(string? name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        PasswordVersion++;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        if (IsDeleted)
        {
            return;
        }

        var now = DateTime.UtcNow;
        IsDeleted = true;
        DeletedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public bool IsLockedOut()
    {
        return LockoutEndUtc.HasValue && LockoutEndUtc.Value > DateTime.UtcNow;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= MaxFailedLoginAttempts)
        {
            LockoutEndUtc = DateTime.UtcNow.Add(LockoutDuration);
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ResetLockout()
    {
        if (FailedLoginAttempts == 0 && !LockoutEndUtc.HasValue)
        {
            return;
        }

        FailedLoginAttempts = 0;
        LockoutEndUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
