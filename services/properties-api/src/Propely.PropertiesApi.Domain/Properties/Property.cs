// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Common;
using Propely.PropertiesApi.Domain.Common.Exceptions;
using Propely.PropertiesApi.Domain.Properties.Events;
using Propely.PropertiesApi.Domain.Properties.ValueObjects;

namespace Propely.PropertiesApi.Domain.Properties;

public sealed class Property : Entity, ISoftDeletable
{
    public const int TitleMaxLength = 200;

    private static readonly Dictionary<PropertyStatus, HashSet<PropertyStatus>> ValidTransitions = new()
    {
        [PropertyStatus.Draft] = [PropertyStatus.Active, PropertyStatus.Archived],
        [PropertyStatus.Active] = [PropertyStatus.Reserved, PropertyStatus.Sold, PropertyStatus.Rented, PropertyStatus.Archived],
        [PropertyStatus.Reserved] = [PropertyStatus.Active, PropertyStatus.Sold, PropertyStatus.Rented, PropertyStatus.Archived],
        [PropertyStatus.Sold] = [],
        [PropertyStatus.Rented] = [],
        [PropertyStatus.Archived] = [PropertyStatus.Draft]
    };

    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public PropertyType PropertyType { get; private set; }
    public OperationType OperationType { get; private set; }
    public PropertyStatus Status { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid AgentId { get; private set; }
    public Guid? AgencyId { get; private set; }
    public LocalizedText? Description { get; private set; }
    public Address? Address { get; private set; }
    public PropertyFeatures? Features { get; private set; }
    public PropertyFinancials? Financials { get; private set; }
    public string? VirtualTourUrl { get; private set; }
    public string? VideoUrl { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    public decimal? PricePerSqm
    {
        get
        {
            var price = Financials?.Price;
            var builtArea = Features?.BuiltArea;
            if (price.HasValue && price.Value > 0 && builtArea.HasValue && builtArea.Value > 0)
                return Math.Round(price.Value / builtArea.Value, 2);
            return null;
        }
    }

    private Property() { }

    public static Property Create(
        string title,
        PropertyType propertyType,
        OperationType operationType,
        Guid tenantId,
        Guid agentId,
        Guid? agencyId = null,
        LocalizedText? description = null,
        Address? address = null,
        PropertyFeatures? features = null,
        PropertyFinancials? financials = null,
        string? virtualTourUrl = null,
        string? videoUrl = null,
        Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Property title is required.");

        var trimmedTitle = title.Trim();
        if (trimmedTitle.Length > TitleMaxLength)
            throw new DomainException($"Property title must not exceed {TitleMaxLength} characters.");

        if (tenantId == Guid.Empty)
            throw new DomainException("Tenant ID is required.");

        if (agentId == Guid.Empty)
            throw new DomainException("Agent ID is required.");

        var now = DateTime.UtcNow;

        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = trimmedTitle,
            PropertyType = propertyType,
            OperationType = operationType,
            Status = PropertyStatus.Draft,
            TenantId = tenantId,
            AgentId = agentId,
            AgencyId = agencyId,
            Description = description,
            Address = address,
            Features = features,
            Financials = financials,
            VirtualTourUrl = virtualTourUrl?.Trim(),
            VideoUrl = videoUrl?.Trim(),
            CreatedAtUtc = now,
            CreatedBy = createdBy
        };

        property.RaiseDomainEvent(new PropertyCreatedV1(
            property.Id, property.TenantId, property.AgentId,
            property.Title, property.PropertyType, property.OperationType, now));

        return property;
    }

    public void Update(
        string? title = null,
        PropertyType? propertyType = null,
        OperationType? operationType = null,
        LocalizedText? description = null,
        Address? address = null,
        PropertyFeatures? features = null,
        PropertyFinancials? financials = null,
        string? virtualTourUrl = null,
        string? videoUrl = null,
        Guid? updatedBy = null)
    {
        if (title is not null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Property title is required.");

            var trimmed = title.Trim();
            if (trimmed.Length > TitleMaxLength)
                throw new DomainException($"Property title must not exceed {TitleMaxLength} characters.");

            Title = trimmed;
        }

        if (propertyType.HasValue) PropertyType = propertyType.Value;
        if (operationType.HasValue) OperationType = operationType.Value;
        if (description is not null) Description = description;
        if (address is not null) Address = address;
        if (features is not null) Features = features;
        if (financials is not null) Financials = financials;
        if (virtualTourUrl is not null) VirtualTourUrl = virtualTourUrl.Trim();
        if (videoUrl is not null) VideoUrl = videoUrl.Trim();

        UpdatedBy = updatedBy;
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new PropertyUpdatedV1(Id, TenantId, AgentId, UpdatedAtUtc.Value));
    }

    public void ChangeStatus(PropertyStatus newStatus)
    {
        if (Status == newStatus)
            throw new DomainException($"Property is already in '{newStatus}' status.");

        if (!ValidTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw new DomainException($"Cannot transition from '{Status}' to '{newStatus}'.");

        var previousStatus = Status;
        var now = DateTime.UtcNow;

        Status = newStatus;
        UpdatedAtUtc = now;

        if (newStatus == PropertyStatus.Active && !PublishedAtUtc.HasValue)
            PublishedAtUtc = now;

        RaiseDomainEvent(new PropertyStatusChangedV1(Id, TenantId, AgentId, previousStatus, newStatus, now));
    }

    public void AssignAgent(Guid agentId)
    {
        if (agentId == Guid.Empty)
            throw new DomainException("Agent ID is required.");

        AgentId = agentId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        var now = DateTime.UtcNow;
        IsDeleted = true;
        DeletedAtUtc = now;
        UpdatedAtUtc = now;

        RaiseDomainEvent(new PropertyDeletedV1(Id, TenantId, AgentId, now));
    }
}
