// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.RegularExpressions;
using Propely.ContactsApi.Domain.Common;
using Propely.ContactsApi.Domain.Common.Exceptions;
using Propely.ContactsApi.Domain.Contacts.Events;
using Propely.ContactsApi.Domain.Contacts.Exceptions;

namespace Propely.ContactsApi.Domain.Contacts;

public sealed partial class Contact : Entity, ISoftDeletable
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 254;
    public const int PhoneMaxLength = 30;
    public const int CompanyMaxLength = 200;
    public const int NotesMaxLength = 2000;

    private readonly List<ContactRole> _roles = [];
    private readonly List<ContactPropertyInterest> _propertyInterests = [];

    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? Phone { get; private set; }
    public string? SecondaryPhone { get; private set; }
    public string? Company { get; private set; }
    public string? Notes { get; private set; }
    public string? PreferredLanguage { get; private set; }
    public ContactSource? Source { get; private set; }
    public Guid? AssignedAgentId { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    public IReadOnlyCollection<ContactRole> Roles => _roles.AsReadOnly();
    public IReadOnlyCollection<ContactPropertyInterest> PropertyInterests => _propertyInterests.AsReadOnly();

    private Contact() { }

    public static Contact Create(
        string firstName,
        string lastName,
        string email,
        Guid tenantId,
        IEnumerable<ContactRole> roles,
        string? phone = null,
        string? secondaryPhone = null,
        string? company = null,
        string? notes = null,
        string? preferredLanguage = null,
        ContactSource? source = null,
        Guid? assignedAgentId = null)
    {
        ValidateFirstName(firstName);
        ValidateLastName(lastName);
        ValidateEmail(email);

        if (tenantId == Guid.Empty)
            throw new ContactValidationException("Tenant ID is required.");

        var roleList = roles?.Distinct().ToList() ?? [];
        if (roleList.Count == 0)
            throw new ContactValidationException("At least one role is required.");

        ValidateOptionalFields(phone, secondaryPhone, company, notes);

        var now = DateTime.UtcNow;

        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            TenantId = tenantId,
            Phone = phone?.Trim(),
            SecondaryPhone = secondaryPhone?.Trim(),
            Company = company?.Trim(),
            Notes = notes?.Trim(),
            PreferredLanguage = preferredLanguage?.Trim(),
            Source = source,
            AssignedAgentId = assignedAgentId,
            CreatedAtUtc = now
        };

        contact._roles.AddRange(roleList);

        contact.RaiseDomainEvent(new ContactCreatedV1(
            contact.Id, contact.TenantId, contact.Email, now));

        return contact;
    }

    public void Update(
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        IEnumerable<ContactRole>? roles = null,
        string? phone = null,
        string? secondaryPhone = null,
        string? company = null,
        string? notes = null,
        string? preferredLanguage = null,
        ContactSource? source = null,
        Guid? assignedAgentId = null)
    {
        if (firstName is not null)
        {
            ValidateFirstName(firstName);
            FirstName = firstName.Trim();
        }

        if (lastName is not null)
        {
            ValidateLastName(lastName);
            LastName = lastName.Trim();
        }

        if (email is not null)
        {
            ValidateEmail(email);
            Email = email.Trim().ToLowerInvariant();
        }

        if (roles is not null)
        {
            var roleList = roles.Distinct().ToList();
            if (roleList.Count == 0)
                throw new ContactValidationException("At least one role is required.");
            _roles.Clear();
            _roles.AddRange(roleList);
        }

        if (phone is not null) Phone = phone.Trim();
        if (secondaryPhone is not null) SecondaryPhone = secondaryPhone.Trim();
        if (company is not null) Company = company.Trim();
        if (notes is not null) Notes = notes.Trim();
        if (preferredLanguage is not null) PreferredLanguage = preferredLanguage.Trim();
        if (source.HasValue) Source = source.Value;
        if (assignedAgentId.HasValue) AssignedAgentId = assignedAgentId.Value;

        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new ContactUpdatedV1(Id, TenantId, UpdatedAtUtc.Value));
    }

    public void SoftDelete()
    {
        var now = DateTime.UtcNow;
        IsDeleted = true;
        DeletedAtUtc = now;
        UpdatedAtUtc = now;

        RaiseDomainEvent(new ContactDeletedV1(Id, TenantId, now));
    }

    public void AddPropertyInterest(Guid propertyId, InterestType interestType, string? notes = null)
    {
        if (_propertyInterests.Any(pi =>
            pi.PropertyId == propertyId && pi.InterestType == interestType))
        {
            throw new DomainException(
                $"Contact already has a '{interestType}' interest for property '{propertyId}'.");
        }

        var interest = ContactPropertyInterest.Create(Id, propertyId, interestType, notes);
        _propertyInterests.Add(interest);
    }

    public void RemovePropertyInterest(Guid propertyId, InterestType interestType)
    {
        var interest = _propertyInterests.FirstOrDefault(pi =>
            pi.PropertyId == propertyId && pi.InterestType == interestType);

        if (interest is null)
        {
            throw new DomainException(
                $"Contact does not have a '{interestType}' interest for property '{propertyId}'.");
        }

        _propertyInterests.Remove(interest);
    }

    private static void ValidateFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ContactValidationException("First name is required.");

        if (firstName.Trim().Length > NameMaxLength)
            throw new ContactValidationException($"First name must not exceed {NameMaxLength} characters.");
    }

    private static void ValidateLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ContactValidationException("Last name is required.");

        if (lastName.Trim().Length > NameMaxLength)
            throw new ContactValidationException($"Last name must not exceed {NameMaxLength} characters.");
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ContactValidationException("Email is required.");

        var trimmed = email.Trim();
        if (trimmed.Length > EmailMaxLength)
            throw new ContactValidationException($"Email must not exceed {EmailMaxLength} characters.");

        if (!EmailRegex().IsMatch(trimmed))
            throw new ContactValidationException("Email format is invalid.");
    }

    private static void ValidateOptionalFields(
        string? phone, string? secondaryPhone, string? company, string? notes)
    {
        if (phone is not null && phone.Trim().Length > PhoneMaxLength)
            throw new ContactValidationException($"Phone must not exceed {PhoneMaxLength} characters.");

        if (secondaryPhone is not null && secondaryPhone.Trim().Length > PhoneMaxLength)
            throw new ContactValidationException($"Secondary phone must not exceed {PhoneMaxLength} characters.");

        if (company is not null && company.Trim().Length > CompanyMaxLength)
            throw new ContactValidationException($"Company must not exceed {CompanyMaxLength} characters.");

        if (notes is not null && notes.Trim().Length > NotesMaxLength)
            throw new ContactValidationException($"Notes must not exceed {NotesMaxLength} characters.");
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
