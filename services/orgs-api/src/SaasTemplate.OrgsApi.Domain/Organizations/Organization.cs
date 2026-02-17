// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Domain.Common;

namespace SaasTemplate.OrgsApi.Domain.Organizations;

public sealed class Organization : Entity, ISoftDeletable
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 500;

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    private Organization() { }

    public static Organization Create(string name)
    {
        return new Organization
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description)
    {
        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAtUtc = DeletedAtUtc = DateTime.UtcNow;
    }
}
