// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Agencies.Events;
using Propely.OrgsApi.Domain.Common;
using Propely.OrgsApi.Domain.Common.Exceptions;

namespace Propely.OrgsApi.Domain.Agencies;

/// <summary>
/// Aggregate root representing a real estate agency that owns one or more branches (organizations).
/// </summary>
public sealed class Agency : Entity, ISoftDeletable
{
    public const int NameMinLength = 1;
    public const int NameMaxLength = 200;

    private readonly List<Guid> _branchIds = [];

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public AgencySlug Slug { get; private set; } = null!;
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    public IReadOnlyList<Guid> BranchIds => _branchIds.AsReadOnly();

    private Agency() { }

    public static Agency Create(string name, string slug, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Agency name is required.");

        var trimmedName = name.Trim();

        if (trimmedName.Length > NameMaxLength)
            throw new DomainException($"Agency name must not exceed {NameMaxLength} characters.");

        var agencySlug = AgencySlug.Create(slug);
        var now = DateTime.UtcNow;

        var agency = new Agency
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            Slug = agencySlug,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = now
        };

        agency.RaiseDomainEvent(new AgencyCreatedV1(
            agency.Id, agency.Name, agency.Slug.Value, agency.CreatedByUserId, now));

        return agency;
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Agency name is required.");

        var trimmedName = name.Trim();

        if (trimmedName.Length > NameMaxLength)
            throw new DomainException($"Agency name must not exceed {NameMaxLength} characters.");

        Name = trimmedName;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddBranch(Guid organizationId)
    {
        if (_branchIds.Contains(organizationId))
            throw new DomainException("This branch is already part of the agency.");

        _branchIds.Add(organizationId);
        var now = DateTime.UtcNow;
        UpdatedAtUtc = now;

        RaiseDomainEvent(new BranchAddedToAgencyV1(Id, organizationId, now));
    }

    public void RemoveBranch(Guid organizationId)
    {
        if (!_branchIds.Contains(organizationId))
            throw new NotFoundException("This branch is not part of the agency.");

        _branchIds.Remove(organizationId);
        var now = DateTime.UtcNow;
        UpdatedAtUtc = now;

        RaiseDomainEvent(new BranchRemovedFromAgencyV1(Id, organizationId, now));
    }

    /// <summary>
    /// Hydrates the branch list from persistence. Does not raise domain events.
    /// </summary>
    internal void HydrateBranches(IEnumerable<Guid> branchIds)
    {
        _branchIds.Clear();
        _branchIds.AddRange(branchIds);
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAtUtc = DeletedAtUtc = DateTime.UtcNow;
    }
}
