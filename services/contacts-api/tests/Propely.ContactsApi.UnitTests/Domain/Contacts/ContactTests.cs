// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.ContactsApi.Domain.Common.Exceptions;
using Propely.ContactsApi.Domain.Contacts;
using Propely.ContactsApi.Domain.Contacts.Events;
using Propely.ContactsApi.Domain.Contacts.Exceptions;

namespace Propely.ContactsApi.UnitTests.Domain.Contacts;

public class ContactTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    private static Contact CreateValidContact(
        string firstName = "Maria",
        string lastName = "Garcia",
        string email = "maria@example.com",
        Guid? tenantId = null,
        IEnumerable<ContactRole>? roles = null)
    {
        return Contact.Create(
            firstName: firstName,
            lastName: lastName,
            email: email,
            tenantId: tenantId ?? TenantId,
            roles: roles ?? [ContactRole.Buyer],
            phone: "+34 650 123 456",
            company: "Inmobiliaria Garcia",
            source: ContactSource.Portal,
            assignedAgentId: AgentId);
    }

    [Fact]
    public void Create_WithValidData_ReturnsContactAndRaisesEvent()
    {
        var contact = CreateValidContact();

        contact.Id.Should().NotBe(Guid.Empty);
        contact.FirstName.Should().Be("Maria");
        contact.LastName.Should().Be("Garcia");
        contact.Email.Should().Be("maria@example.com");
        contact.TenantId.Should().Be(TenantId);
        contact.Phone.Should().Be("+34 650 123 456");
        contact.Company.Should().Be("Inmobiliaria Garcia");
        contact.Source.Should().Be(ContactSource.Portal);
        contact.AssignedAgentId.Should().Be(AgentId);
        contact.Roles.Should().ContainSingle().Which.Should().Be(ContactRole.Buyer);
        contact.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        contact.IsDeleted.Should().BeFalse();

        contact.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ContactCreatedV1>()
            .Which.Data.ContactId.Should().Be(contact.Id);
    }

    [Fact]
    public void Create_WithMultipleRoles_StoresAllRoles()
    {
        var roles = new[] { ContactRole.Buyer, ContactRole.Seller, ContactRole.Landlord };
        var contact = CreateValidContact(roles: roles);

        contact.Roles.Should().HaveCount(3);
        contact.Roles.Should().Contain(ContactRole.Buyer);
        contact.Roles.Should().Contain(ContactRole.Seller);
        contact.Roles.Should().Contain(ContactRole.Landlord);
    }

    [Fact]
    public void Create_WithDuplicateRoles_DeduplicatesRoles()
    {
        var roles = new[] { ContactRole.Buyer, ContactRole.Buyer, ContactRole.Seller };
        var contact = CreateValidContact(roles: roles);

        contact.Roles.Should().HaveCount(2);
    }

    [Fact]
    public void Create_NormalizesEmailToLowerCase()
    {
        var contact = CreateValidContact(email: "MARIA@EXAMPLE.COM");
        contact.Email.Should().Be("maria@example.com");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var contact = CreateValidContact(firstName: "  Maria  ", lastName: "  Garcia  ", email: "  maria@example.com  ");
        contact.FirstName.Should().Be("Maria");
        contact.LastName.Should().Be("Garcia");
        contact.Email.Should().Be("maria@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyFirstName_Throws(string? firstName)
    {
        var act = () => CreateValidContact(firstName: firstName!);
        act.Should().Throw<ContactValidationException>().WithMessage("*First name*required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyLastName_Throws(string? lastName)
    {
        var act = () => CreateValidContact(lastName: lastName!);
        act.Should().Throw<ContactValidationException>().WithMessage("*Last name*required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_Throws(string? email)
    {
        var act = () => CreateValidContact(email: email!);
        act.Should().Throw<ContactValidationException>().WithMessage("*Email*required*");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@tld")]
    [InlineData("@nodomain.com")]
    public void Create_WithInvalidEmailFormat_Throws(string email)
    {
        var act = () => CreateValidContact(email: email);
        act.Should().Throw<ContactValidationException>().WithMessage("*Email format*invalid*");
    }

    [Fact]
    public void Create_WithEmptyTenantId_Throws()
    {
        var act = () => CreateValidContact(tenantId: Guid.Empty);
        act.Should().Throw<ContactValidationException>().WithMessage("*Tenant ID*required*");
    }

    [Fact]
    public void Create_WithNoRoles_Throws()
    {
        var act = () => CreateValidContact(roles: []);
        act.Should().Throw<ContactValidationException>().WithMessage("*At least one role*required*");
    }

    [Fact]
    public void Create_WithNullRoles_Throws()
    {
        var act = () => Contact.Create("Maria", "Garcia", "m@e.com", TenantId, null!);
        act.Should().Throw<ContactValidationException>().WithMessage("*At least one role*required*");
    }

    [Fact]
    public void Update_ChangesFieldsAndRaisesEvent()
    {
        var contact = CreateValidContact();
        contact.ClearDomainEvents();

        contact.Update(
            firstName: "Updated",
            lastName: "Name",
            email: "updated@example.com",
            roles: [ContactRole.Tenant]);

        contact.FirstName.Should().Be("Updated");
        contact.LastName.Should().Be("Name");
        contact.Email.Should().Be("updated@example.com");
        contact.Roles.Should().ContainSingle().Which.Should().Be(ContactRole.Tenant);
        contact.UpdatedAtUtc.Should().NotBeNull();

        contact.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ContactUpdatedV1>()
            .Which.Data.ContactId.Should().Be(contact.Id);
    }

    [Fact]
    public void Update_WithEmptyRoles_Throws()
    {
        var contact = CreateValidContact();

        var act = () => contact.Update(roles: []);
        act.Should().Throw<ContactValidationException>().WithMessage("*At least one role*required*");
    }

    [Fact]
    public void Update_WithNullOptionalFields_DoesNotOverwrite()
    {
        var contact = CreateValidContact();
        var originalFirstName = contact.FirstName;
        var originalEmail = contact.Email;
        contact.ClearDomainEvents();

        contact.Update(company: "New Company");

        contact.FirstName.Should().Be(originalFirstName);
        contact.Email.Should().Be(originalEmail);
        contact.Company.Should().Be("New Company");
    }

    [Fact]
    public void SoftDelete_SetsIsDeletedAndRaisesEvent()
    {
        var contact = CreateValidContact();
        contact.ClearDomainEvents();

        contact.SoftDelete();

        contact.IsDeleted.Should().BeTrue();
        contact.DeletedAtUtc.Should().NotBeNull();
        contact.UpdatedAtUtc.Should().NotBeNull();

        contact.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ContactDeletedV1>()
            .Which.Data.ContactId.Should().Be(contact.Id);
    }

    [Fact]
    public void AddPropertyInterest_AddsInterest()
    {
        var contact = CreateValidContact();
        var propertyId = Guid.NewGuid();

        contact.AddPropertyInterest(propertyId, InterestType.Buying, "Interested in 3-bed");

        contact.PropertyInterests.Should().ContainSingle();
        var interest = contact.PropertyInterests.First();
        interest.ContactId.Should().Be(contact.Id);
        interest.PropertyId.Should().Be(propertyId);
        interest.InterestType.Should().Be(InterestType.Buying);
        interest.Notes.Should().Be("Interested in 3-bed");
    }

    [Fact]
    public void AddPropertyInterest_DuplicateSameType_Throws()
    {
        var contact = CreateValidContact();
        var propertyId = Guid.NewGuid();

        contact.AddPropertyInterest(propertyId, InterestType.Buying);

        var act = () => contact.AddPropertyInterest(propertyId, InterestType.Buying);
        act.Should().Throw<DomainException>().WithMessage("*already has*interest*");
    }

    [Fact]
    public void AddPropertyInterest_SamePropertyDifferentType_Succeeds()
    {
        var contact = CreateValidContact();
        var propertyId = Guid.NewGuid();

        contact.AddPropertyInterest(propertyId, InterestType.Buying);
        contact.AddPropertyInterest(propertyId, InterestType.Renting);

        contact.PropertyInterests.Should().HaveCount(2);
    }

    [Fact]
    public void RemovePropertyInterest_RemovesCorrectly()
    {
        var contact = CreateValidContact();
        var propertyId = Guid.NewGuid();
        contact.AddPropertyInterest(propertyId, InterestType.Buying);

        contact.RemovePropertyInterest(propertyId, InterestType.Buying);

        contact.PropertyInterests.Should().BeEmpty();
    }

    [Fact]
    public void RemovePropertyInterest_NotFound_Throws()
    {
        var contact = CreateValidContact();

        var act = () => contact.RemovePropertyInterest(Guid.NewGuid(), InterestType.Buying);
        act.Should().Throw<DomainException>().WithMessage("*does not have*interest*");
    }

    [Fact]
    public void DomainEvents_ContainCorrectIdsAndTimestamp()
    {
        var contact = CreateValidContact();
        var evt = contact.DomainEvents.OfType<ContactCreatedV1>().Single();

        evt.EventId.Should().NotBe(Guid.Empty);
        evt.EventType.Should().Be("ContactCreatedV1");
        evt.SchemaVersion.Should().Be(1);
        evt.Producer.Should().Be("ContactsApi");
        evt.OccurredAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        evt.Data.ContactId.Should().Be(contact.Id);
        evt.Data.TenantId.Should().Be(contact.TenantId);
        evt.Data.Email.Should().Be(contact.Email);
    }
}
