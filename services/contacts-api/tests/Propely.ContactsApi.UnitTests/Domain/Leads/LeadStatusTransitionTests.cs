// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.ContactsApi.Domain.Common.Exceptions;
using Propely.ContactsApi.Domain.Leads;
using Propely.ContactsApi.Domain.Leads.Events;

namespace Propely.ContactsApi.UnitTests.Domain.Leads;

public class LeadStatusTransitionTests
{
    private static readonly Guid PropertyId = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();

    private static Lead CreateLeadInStatus(LeadStatus targetStatus)
    {
        var lead = Lead.Create("Test Lead", "test@example.com", PropertyId, TenantId);
        lead.ClearDomainEvents();

        switch (targetStatus)
        {
            case LeadStatus.New:
                break;
            case LeadStatus.Contacted:
                lead.ChangeStatus(LeadStatus.Contacted);
                lead.ClearDomainEvents();
                break;
            case LeadStatus.Qualified:
                lead.ChangeStatus(LeadStatus.Contacted);
                lead.ChangeStatus(LeadStatus.Qualified);
                lead.ClearDomainEvents();
                break;
            case LeadStatus.Converted:
                lead.ChangeStatus(LeadStatus.Contacted);
                lead.ChangeStatus(LeadStatus.Qualified);
                lead.Convert(Guid.NewGuid(), wasNewContact: true);
                lead.ClearDomainEvents();
                break;
            case LeadStatus.Lost:
                lead.ChangeStatus(LeadStatus.Lost);
                lead.ClearDomainEvents();
                break;
        }

        return lead;
    }

    // Valid transitions
    [Theory]
    [InlineData(LeadStatus.New, LeadStatus.Contacted)]
    [InlineData(LeadStatus.New, LeadStatus.Lost)]
    [InlineData(LeadStatus.Contacted, LeadStatus.Qualified)]
    [InlineData(LeadStatus.Contacted, LeadStatus.Lost)]
    [InlineData(LeadStatus.Qualified, LeadStatus.Lost)]
    public void ChangeStatus_ValidTransition_Succeeds(LeadStatus from, LeadStatus to)
    {
        var lead = CreateLeadInStatus(from);

        lead.ChangeStatus(to);

        lead.Status.Should().Be(to);
        lead.UpdatedAtUtc.Should().NotBeNull();
        lead.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<LeadStatusChangedV1>();
    }

    [Fact]
    public void ChangeStatus_ValidTransition_RaisesEventWithCorrectData()
    {
        var lead = CreateLeadInStatus(LeadStatus.New);

        lead.ChangeStatus(LeadStatus.Contacted);

        var evt = lead.DomainEvents.OfType<LeadStatusChangedV1>().Single();
        evt.Data.PreviousStatus.Should().Be(LeadStatus.New);
        evt.Data.NewStatus.Should().Be(LeadStatus.Contacted);
        evt.Data.LeadId.Should().Be(lead.Id);
    }

    // Invalid transitions
    [Theory]
    [InlineData(LeadStatus.New, LeadStatus.Qualified)]
    [InlineData(LeadStatus.New, LeadStatus.Converted)]
    [InlineData(LeadStatus.Contacted, LeadStatus.New)]
    [InlineData(LeadStatus.Contacted, LeadStatus.Converted)]
    [InlineData(LeadStatus.Qualified, LeadStatus.New)]
    [InlineData(LeadStatus.Qualified, LeadStatus.Contacted)]
    [InlineData(LeadStatus.Converted, LeadStatus.New)]
    [InlineData(LeadStatus.Converted, LeadStatus.Contacted)]
    [InlineData(LeadStatus.Converted, LeadStatus.Qualified)]
    [InlineData(LeadStatus.Converted, LeadStatus.Lost)]
    [InlineData(LeadStatus.Lost, LeadStatus.New)]
    [InlineData(LeadStatus.Lost, LeadStatus.Contacted)]
    [InlineData(LeadStatus.Lost, LeadStatus.Qualified)]
    [InlineData(LeadStatus.Lost, LeadStatus.Converted)]
    public void ChangeStatus_InvalidTransition_Throws(LeadStatus from, LeadStatus to)
    {
        var lead = CreateLeadInStatus(from);

        var act = () => lead.ChangeStatus(to);
        act.Should().Throw<DomainException>().WithMessage($"*Cannot transition from*'{from}'*to*'{to}'*");
    }

    [Theory]
    [InlineData(LeadStatus.New)]
    [InlineData(LeadStatus.Contacted)]
    [InlineData(LeadStatus.Qualified)]
    public void ChangeStatus_ToSameStatus_Throws(LeadStatus status)
    {
        var lead = CreateLeadInStatus(status);

        var act = () => lead.ChangeStatus(status);
        act.Should().Throw<DomainException>().WithMessage($"*already in*'{status}'*");
    }

    // Convert tests
    [Fact]
    public void Convert_FromQualified_Succeeds()
    {
        var lead = CreateLeadInStatus(LeadStatus.Qualified);
        var contactId = Guid.NewGuid();

        lead.Convert(contactId, wasNewContact: true);

        lead.Status.Should().Be(LeadStatus.Converted);
        lead.ContactId.Should().Be(contactId);
        lead.UpdatedAtUtc.Should().NotBeNull();

        lead.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<LeadConvertedV1>();
    }

    [Fact]
    public void Convert_RaisesEventWithCorrectData()
    {
        var lead = CreateLeadInStatus(LeadStatus.Qualified);
        var contactId = Guid.NewGuid();

        lead.Convert(contactId, wasNewContact: false);

        var evt = lead.DomainEvents.OfType<LeadConvertedV1>().Single();
        evt.Data.LeadId.Should().Be(lead.Id);
        evt.Data.ContactId.Should().Be(contactId);
        evt.Data.PropertyId.Should().Be(lead.PropertyId);
        evt.Data.WasNewContact.Should().BeFalse();
    }

    [Fact]
    public void Convert_WithEmptyContactId_Throws()
    {
        var lead = CreateLeadInStatus(LeadStatus.Qualified);

        var act = () => lead.Convert(Guid.Empty, wasNewContact: true);
        act.Should().Throw<DomainException>().WithMessage("*Contact ID*required*");
    }

    [Fact]
    public void Convert_WhenAlreadyConverted_Throws()
    {
        var lead = CreateLeadInStatus(LeadStatus.Converted);

        var act = () => lead.Convert(Guid.NewGuid(), wasNewContact: true);
        act.Should().Throw<DomainException>().WithMessage("*already converted*");
    }

    [Fact]
    public void Convert_WhenLost_Throws()
    {
        var lead = CreateLeadInStatus(LeadStatus.Lost);

        var act = () => lead.Convert(Guid.NewGuid(), wasNewContact: true);
        act.Should().Throw<DomainException>().WithMessage("*Cannot convert a lost lead*");
    }

    [Theory]
    [InlineData(LeadStatus.New)]
    [InlineData(LeadStatus.Contacted)]
    public void Convert_WhenNotQualified_Throws(LeadStatus status)
    {
        var lead = CreateLeadInStatus(status);

        var act = () => lead.Convert(Guid.NewGuid(), wasNewContact: true);
        act.Should().Throw<DomainException>().WithMessage("*must be in 'Qualified' status*");
    }
}
