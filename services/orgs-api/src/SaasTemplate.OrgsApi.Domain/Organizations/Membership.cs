// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Domain.Common;

namespace SaasTemplate.OrgsApi.Domain.Organizations;

public sealed class Membership : Entity, ISoftDeletable
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public MembershipRole Role { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    private Membership() { }

    public static Membership Create(Guid userId, Guid organizationId, MembershipRole role)
    {
        return new Membership
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrganizationId = organizationId,
            Role = role,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdateRole(MembershipRole newRole)
    {
        Role = newRole;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
    }
}
