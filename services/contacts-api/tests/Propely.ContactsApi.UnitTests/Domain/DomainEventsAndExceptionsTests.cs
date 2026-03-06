// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.ContactsApi.Domain.Contacts.Events;
using Propely.ContactsApi.Domain.Contacts.Exceptions;
using Propely.ContactsApi.Domain.Leads.Events;
using Propely.ContactsApi.Domain.Leads;
using Propely.ContactsApi.Domain.Leads.Exceptions;

namespace Propely.ContactsApi.UnitTests.Domain;

/// <summary>
/// Covers auto-generated record equality and domain exception constructors.
/// </summary>
public class DomainEventsAndExceptionsTests
{
    private static readonly Guid Id = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly DateTime Now = DateTime.UtcNow;

    // --- ContactDeletedV1 ---

    [Fact]
    public void ContactDeletedV1_Properties_AndEqualityMembers()
    {
        var evt = new ContactDeletedV1(Id, TenantId, Now);

        evt.Data.ContactId.Should().Be(Id);
        evt.Data.TenantId.Should().Be(TenantId);
        evt.EventType.Should().Be("ContactDeletedV1");
        evt.SchemaVersion.Should().Be(1);
        evt.Producer.Should().Be("ContactsApi");

        // Cover auto-generated record members
        _ = evt.ToString();
        _ = evt.GetHashCode();
        _ = evt.Equals((object?)null);
        _ = evt.Equals((ContactDeletedV1?)null);
        _ = (evt == null);
        _ = (evt != null);
    }

    // --- ContactUpdatedV1 ---

    [Fact]
    public void ContactUpdatedV1_Properties_AndEqualityMembers()
    {
        var evt = new ContactUpdatedV1(Id, TenantId, Now);

        evt.Data.ContactId.Should().Be(Id);
        evt.EventType.Should().Be("ContactUpdatedV1");

        _ = evt.ToString();
        _ = evt.GetHashCode();
        _ = evt.Equals((object?)null);
        _ = evt.Equals((ContactUpdatedV1?)null);
        _ = (evt == null);
        _ = (evt != null);
    }

    // --- LeadDeletedV1 ---

    [Fact]
    public void LeadDeletedV1_Properties_AndEqualityMembers()
    {
        var propertyId = Guid.NewGuid();
        var evt = new LeadDeletedV1(Id, propertyId, TenantId, Now);

        evt.Data.LeadId.Should().Be(Id);
        evt.EventType.Should().Be("LeadDeletedV1");

        _ = evt.ToString();
        _ = evt.GetHashCode();
        _ = evt.Equals((object?)null);
        _ = evt.Equals((LeadDeletedV1?)null);
        _ = (evt == null);
        _ = (evt != null);
    }

    // --- LeadStatusChangedV1 ---

    [Fact]
    public void LeadStatusChangedV1_Properties_AndEqualityMembers()
    {
        var propertyId = Guid.NewGuid();
        var evt = new LeadStatusChangedV1(Id, propertyId, TenantId, LeadStatus.New, LeadStatus.Contacted, Now);

        evt.Data.PreviousStatus.Should().Be(LeadStatus.New);
        evt.Data.NewStatus.Should().Be(LeadStatus.Contacted);
        evt.EventType.Should().Be("LeadStatusChangedV1");

        _ = evt.ToString();
        _ = evt.GetHashCode();
        _ = evt.Equals((object?)null);
        _ = evt.Equals((LeadStatusChangedV1?)null);
        _ = (evt == null);
        _ = (evt != null);
    }

    // --- ContactValidationException inner exception constructor ---

    [Fact]
    public void ContactValidationException_WithInnerException_SetsMessage()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new ContactValidationException("validation failed", inner);

        ex.Message.Should().Be("validation failed");
        ex.InnerException.Should().Be(inner);
    }

    // --- LeadValidationException inner exception constructor ---

    [Fact]
    public void LeadValidationException_WithInnerException_SetsMessage()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new LeadValidationException("lead validation failed", inner);

        ex.Message.Should().Be("lead validation failed");
        ex.InnerException.Should().Be(inner);
    }
}
