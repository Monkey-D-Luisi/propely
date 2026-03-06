// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.RegularExpressions;
using Propely.ContactsApi.Domain.Common;
using Propely.ContactsApi.Domain.Common.Exceptions;
using Propely.ContactsApi.Domain.Leads.Events;
using Propely.ContactsApi.Domain.Leads.Exceptions;

namespace Propely.ContactsApi.Domain.Leads;

public sealed partial class Lead : Entity, ISoftDeletable
{
    public const int NameMaxLength = 200;
    public const int EmailMaxLength = 254;
    public const int PhoneMaxLength = 30;
    public const int MessageMaxLength = 2000;
    public const int SourceMaxLength = 100;

    private static readonly Dictionary<LeadStatus, HashSet<LeadStatus>> ValidTransitions = new()
    {
        [LeadStatus.New] = [LeadStatus.Contacted, LeadStatus.Lost],
        [LeadStatus.Contacted] = [LeadStatus.Qualified, LeadStatus.Lost],
        [LeadStatus.Qualified] = [LeadStatus.Converted, LeadStatus.Lost],
        [LeadStatus.Converted] = [],
        [LeadStatus.Lost] = []
    };

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? Phone { get; private set; }
    public string? Message { get; private set; }
    public string? Source { get; private set; }
    public Guid PropertyId { get; private set; }
    public Guid TenantId { get; private set; }
    public LeadStatus Status { get; private set; }
    public Guid? AssignedAgentId { get; private set; }
    public Guid? ContactId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    private Lead() { }

    public static Lead Create(
        string name,
        string email,
        Guid propertyId,
        Guid tenantId,
        string? phone = null,
        string? message = null,
        string? source = null,
        Guid? assignedAgentId = null)
    {
        ValidateName(name);
        ValidateEmail(email);

        if (propertyId == Guid.Empty)
            throw new LeadValidationException("Property ID is required.");

        if (tenantId == Guid.Empty)
            throw new LeadValidationException("Tenant ID is required.");

        ValidateOptionalFields(phone, message, source);

        var now = DateTime.UtcNow;

        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PropertyId = propertyId,
            TenantId = tenantId,
            Phone = phone?.Trim(),
            Message = message?.Trim(),
            Source = source?.Trim(),
            AssignedAgentId = assignedAgentId,
            Status = LeadStatus.New,
            CreatedAtUtc = now
        };

        lead.RaiseDomainEvent(new LeadCreatedV1(
            lead.Id, lead.PropertyId, lead.TenantId, lead.Email, now));

        return lead;
    }

    public void ChangeStatus(LeadStatus newStatus)
    {
        if (Status == newStatus)
            throw new DomainException($"Lead is already in '{newStatus}' status.");

        if (!ValidTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw new DomainException($"Cannot transition from '{Status}' to '{newStatus}'.");

        var previousStatus = Status;
        var now = DateTime.UtcNow;

        Status = newStatus;
        UpdatedAtUtc = now;

        RaiseDomainEvent(new LeadStatusChangedV1(
            Id, PropertyId, TenantId, previousStatus, newStatus, now));
    }

    public void Convert(Guid contactId, bool wasNewContact)
    {
        if (contactId == Guid.Empty)
            throw new DomainException("Contact ID is required for conversion.");

        if (Status == LeadStatus.Converted)
            throw new DomainException("Lead is already converted.");

        if (Status == LeadStatus.Lost)
            throw new DomainException("Cannot convert a lost lead.");

        if (Status != LeadStatus.Qualified)
            throw new DomainException($"Lead must be in 'Qualified' status to convert, but is in '{Status}'.");

        var now = DateTime.UtcNow;

        Status = LeadStatus.Converted;
        ContactId = contactId;
        UpdatedAtUtc = now;

        RaiseDomainEvent(new LeadConvertedV1(
            Id, contactId, PropertyId, TenantId, wasNewContact, now));
    }

    public void AssignAgent(Guid agentId)
    {
        if (agentId == Guid.Empty)
            throw new DomainException("Agent ID is required.");

        AssignedAgentId = agentId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        var now = DateTime.UtcNow;
        IsDeleted = true;
        DeletedAtUtc = now;
        UpdatedAtUtc = now;

        RaiseDomainEvent(new LeadDeletedV1(Id, PropertyId, TenantId, now));
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new LeadValidationException("Name is required.");

        if (name.Trim().Length > NameMaxLength)
            throw new LeadValidationException($"Name must not exceed {NameMaxLength} characters.");
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new LeadValidationException("Email is required.");

        var trimmed = email.Trim();
        if (trimmed.Length > EmailMaxLength)
            throw new LeadValidationException($"Email must not exceed {EmailMaxLength} characters.");

        if (!EmailRegex().IsMatch(trimmed))
            throw new LeadValidationException("Email format is invalid.");
    }

    private static void ValidateOptionalFields(string? phone, string? message, string? source)
    {
        if (phone is not null && phone.Trim().Length > PhoneMaxLength)
            throw new LeadValidationException($"Phone must not exceed {PhoneMaxLength} characters.");

        if (message is not null && message.Trim().Length > MessageMaxLength)
            throw new LeadValidationException($"Message must not exceed {MessageMaxLength} characters.");

        if (source is not null && source.Trim().Length > SourceMaxLength)
            throw new LeadValidationException($"Source must not exceed {SourceMaxLength} characters.");
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
