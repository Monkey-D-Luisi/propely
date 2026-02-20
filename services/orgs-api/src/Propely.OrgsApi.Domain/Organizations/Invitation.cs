// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;

namespace Propely.OrgsApi.Domain.Organizations;

public sealed class Invitation : Entity, ISoftDeletable
{
    public const int TokenLength = 64;

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string Email { get; private set; } = null!;
    public MembershipRole Role { get; private set; }
    public string Token { get; private set; } = null!;
    public InvitationStatus Status { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    private Invitation() { }

    public static Invitation Create(Guid organizationId, string email, MembershipRole role, TimeSpan? expiry = null)
    {
        var token = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));

        return new Invitation
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Email = email.Trim().ToLowerInvariant(),
            Role = role,
            Token = token,
            Status = InvitationStatus.Pending,
            ExpiresAtUtc = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromDays(7)),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Accept()
    {
        Status = InvitationStatus.Accepted;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
    }
}
