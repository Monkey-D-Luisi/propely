// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.ContactsApi.Domain.Common.Exceptions;
using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.UnitTests.Domain.Contacts;

public class ContactPropertyInterestTests
{
    [Fact]
    public void Create_WithValidData_ReturnsInterest()
    {
        var contactId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        var interest = ContactPropertyInterest.Create(contactId, propertyId, InterestType.Buying, "Nice flat");

        interest.Id.Should().NotBe(Guid.Empty);
        interest.ContactId.Should().Be(contactId);
        interest.PropertyId.Should().Be(propertyId);
        interest.InterestType.Should().Be(InterestType.Buying);
        interest.Notes.Should().Be("Nice flat");
        interest.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithEmptyContactId_Throws()
    {
        var act = () => ContactPropertyInterest.Create(Guid.Empty, Guid.NewGuid(), InterestType.Buying);
        act.Should().Throw<DomainException>().WithMessage("*Contact ID*required*");
    }

    [Fact]
    public void Create_WithEmptyPropertyId_Throws()
    {
        var act = () => ContactPropertyInterest.Create(Guid.NewGuid(), Guid.Empty, InterestType.Buying);
        act.Should().Throw<DomainException>().WithMessage("*Property ID*required*");
    }

    [Fact]
    public void Create_TrimsNotes()
    {
        var interest = ContactPropertyInterest.Create(Guid.NewGuid(), Guid.NewGuid(), InterestType.Renting, "  notes  ");
        interest.Notes.Should().Be("notes");
    }

    [Fact]
    public void Create_WithNullNotes_SetsNull()
    {
        var interest = ContactPropertyInterest.Create(Guid.NewGuid(), Guid.NewGuid(), InterestType.Selling);
        interest.Notes.Should().BeNull();
    }
}
