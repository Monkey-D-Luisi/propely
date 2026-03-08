// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.AiApi.Application.Actions.Commands.ContactActions;
using Propely.AiApi.Application.Actions.Handlers.ContactActions;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;
using Propely.ContactsApi.Client;
using Propely.ContactsApi.Client.Models;

namespace Propely.AiApi.UnitTests.Application.Actions.Handlers.ContactActions;

public sealed class ConvertLeadActionHandlerTests
{
    private readonly ILeadsApiClient _leadsClient;
    private readonly ConvertLeadActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public ConvertLeadActionHandlerTests()
    {
        _leadsClient = Substitute.For<ILeadsApiClient>();
        var logger = Substitute.For<ILogger<ConvertLeadActionHandler>>();
        _handler = new ConvertLeadActionHandler(_leadsClient, logger);
    }

    [Fact]
    public async Task Handle_WithValidLeadId_ShouldConvertLeadViaSDK()
    {
        var leadId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var parameters = new ConvertLeadParameters(
            LeadId: leadId,
            Role: null,
            Notes: null);
        var command = new ConvertLeadActionCommand(parameters, TenantId, AgentId);

        _leadsClient.ConvertLeadAsync(leadId, Arg.Any<ConvertLeadClientRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ConvertLeadClientResponse
            {
                Lead = new LeadResponse { Id = leadId, Name = "Maria Garcia", Email = "maria@example.com", Status = "Converted" },
                Contact = new ContactResponse { Id = contactId, FirstName = "Maria", LastName = "Garcia", Email = "maria@example.com" },
                WasNewContact = true
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.ConvertLead);
        result.Message.Should().Contain("Maria Garcia");
        result.Message.Should().Contain("new contact");
        await _leadsClient.Received(1).ConvertLeadAsync(
            leadId,
            Arg.Is<ConvertLeadClientRequest>(r => r.Role == "Buyer"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCustomRole_ShouldPassRoleToSDK()
    {
        var leadId = Guid.NewGuid();
        var parameters = new ConvertLeadParameters(
            LeadId: leadId,
            Role: "Seller",
            Notes: null);
        var command = new ConvertLeadActionCommand(parameters, TenantId, AgentId);

        _leadsClient.ConvertLeadAsync(leadId, Arg.Any<ConvertLeadClientRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ConvertLeadClientResponse
            {
                Lead = new LeadResponse { Id = leadId, Name = "Test", Email = "t@t.com", Status = "Converted" },
                Contact = new ContactResponse { Id = Guid.NewGuid(), FirstName = "Test", LastName = "User", Email = "t@t.com" },
                WasNewContact = false
            });

        await _handler.Handle(command, CancellationToken.None);

        await _leadsClient.Received(1).ConvertLeadAsync(
            leadId,
            Arg.Is<ConvertLeadClientRequest>(r => r.Role == "Seller"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExistingContact_ShouldIndicateExisting()
    {
        var leadId = Guid.NewGuid();
        var parameters = new ConvertLeadParameters(
            LeadId: leadId,
            Role: null,
            Notes: null);
        var command = new ConvertLeadActionCommand(parameters, TenantId, AgentId);

        _leadsClient.ConvertLeadAsync(leadId, Arg.Any<ConvertLeadClientRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ConvertLeadClientResponse
            {
                Lead = new LeadResponse { Id = leadId, Name = "Test", Email = "t@t.com", Status = "Converted" },
                Contact = new ContactResponse { Id = Guid.NewGuid(), FirstName = "Test", LastName = "User", Email = "t@t.com" },
                WasNewContact = false
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("existing contact");
    }

    [Fact]
    public async Task Handle_WithMissingLeadId_ShouldReturnFailure()
    {
        var parameters = new ConvertLeadParameters(
            LeadId: null,
            Role: null,
            Notes: null);
        var command = new ConvertLeadActionCommand(parameters, TenantId, AgentId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Lead ID is required to convert a lead.");
    }

    [Fact]
    public async Task Handle_WhenSdkThrows_ShouldReturnFailureGracefully()
    {
        var leadId = Guid.NewGuid();
        var parameters = new ConvertLeadParameters(
            LeadId: leadId,
            Role: null,
            Notes: null);
        var command = new ConvertLeadActionCommand(parameters, TenantId, AgentId);

        _leadsClient.ConvertLeadAsync(leadId, Arg.Any<ConvertLeadClientRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}
