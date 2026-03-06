// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.ContactsApi.Domain.Common.Exceptions;
using Propely.ContactsApi.Domain.Leads;
using Propely.ContactsApi.Domain.Leads.Events;
using Propely.ContactsApi.Domain.Leads.Exceptions;

namespace Propely.ContactsApi.UnitTests.Domain.Leads;

public class LeadTests
{
    private static readonly Guid PropertyId = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    private static Lead CreateValidLead(
        string name = "Maria Garcia",
        string email = "maria@example.com",
        Guid? propertyId = null,
        Guid? tenantId = null,
        string? source = null)
    {
        return Lead.Create(
            name: name,
            email: email,
            propertyId: propertyId ?? PropertyId,
            tenantId: tenantId ?? TenantId,
            phone: "+34 650 123 456",
            message: "Interested in the apartment",
            source: source ?? "Idealista",
            assignedAgentId: AgentId);
    }

    [Fact]
    public void Create_WithValidData_ReturnsLeadWithNewStatusAndRaisesEvent()
    {
        var lead = CreateValidLead();

        lead.Id.Should().NotBe(Guid.Empty);
        lead.Name.Should().Be("Maria Garcia");
        lead.Email.Should().Be("maria@example.com");
        lead.PropertyId.Should().Be(PropertyId);
        lead.TenantId.Should().Be(TenantId);
        lead.Status.Should().Be(LeadStatus.New);
        lead.Phone.Should().Be("+34 650 123 456");
        lead.Message.Should().Be("Interested in the apartment");
        lead.Source.Should().Be("Idealista");
        lead.AssignedAgentId.Should().Be(AgentId);
        lead.ContactId.Should().BeNull();
        lead.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        lead.IsDeleted.Should().BeFalse();

        lead.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<LeadCreatedV1>()
            .Which.Data.LeadId.Should().Be(lead.Id);
    }

    [Fact]
    public void Create_NormalizesEmailToLowerCase()
    {
        var lead = CreateValidLead(email: "MARIA@EXAMPLE.COM");
        lead.Email.Should().Be("maria@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyName_Throws(string? name)
    {
        var act = () => CreateValidLead(name: name!);
        act.Should().Throw<LeadValidationException>().WithMessage("*Name*required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_Throws(string? email)
    {
        var act = () => CreateValidLead(email: email!);
        act.Should().Throw<LeadValidationException>().WithMessage("*Email*required*");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@tld")]
    [InlineData("@nodomain.com")]
    public void Create_WithInvalidEmailFormat_Throws(string email)
    {
        var act = () => CreateValidLead(email: email);
        act.Should().Throw<LeadValidationException>().WithMessage("*Email format*invalid*");
    }

    [Fact]
    public void Create_WithEmptyPropertyId_Throws()
    {
        var act = () => CreateValidLead(propertyId: Guid.Empty);
        act.Should().Throw<LeadValidationException>().WithMessage("*Property ID*required*");
    }

    [Fact]
    public void Create_WithEmptyTenantId_Throws()
    {
        var act = () => CreateValidLead(tenantId: Guid.Empty);
        act.Should().Throw<LeadValidationException>().WithMessage("*Tenant ID*required*");
    }

    [Fact]
    public void AssignAgent_SetsAgent()
    {
        var lead = CreateValidLead();
        var newAgentId = Guid.NewGuid();

        lead.AssignAgent(newAgentId);

        lead.AssignedAgentId.Should().Be(newAgentId);
        lead.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void AssignAgent_WithEmptyId_Throws()
    {
        var lead = CreateValidLead();
        var act = () => lead.AssignAgent(Guid.Empty);
        act.Should().Throw<DomainException>().WithMessage("*Agent ID*required*");
    }

    [Fact]
    public void SoftDelete_SetsIsDeletedAndRaisesEvent()
    {
        var lead = CreateValidLead();
        lead.ClearDomainEvents();

        lead.SoftDelete();

        lead.IsDeleted.Should().BeTrue();
        lead.DeletedAtUtc.Should().NotBeNull();

        lead.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<LeadDeletedV1>()
            .Which.Data.LeadId.Should().Be(lead.Id);
    }

    [Fact]
    public void DomainEvents_ContainCorrectIdsAndTimestamp()
    {
        var lead = CreateValidLead();
        var evt = lead.DomainEvents.OfType<LeadCreatedV1>().Single();

        evt.EventId.Should().NotBe(Guid.Empty);
        evt.EventType.Should().Be("LeadCreatedV1");
        evt.SchemaVersion.Should().Be(1);
        evt.Producer.Should().Be("ContactsApi");
        evt.OccurredAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        evt.Data.LeadId.Should().Be(lead.Id);
        evt.Data.PropertyId.Should().Be(lead.PropertyId);
        evt.Data.TenantId.Should().Be(lead.TenantId);
        evt.Data.Email.Should().Be(lead.Email);
    }
}
