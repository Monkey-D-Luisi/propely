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

public sealed class QualifyLeadActionHandlerTests
{
    private readonly ILeadsApiClient _leadsClient;
    private readonly QualifyLeadActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public QualifyLeadActionHandlerTests()
    {
        _leadsClient = Substitute.For<ILeadsApiClient>();
        var logger = Substitute.For<ILogger<QualifyLeadActionHandler>>();
        _handler = new QualifyLeadActionHandler(_leadsClient, logger);
    }

    [Fact]
    public async Task Handle_WithValidLeadId_ShouldChangeStatusToQualified()
    {
        var leadId = Guid.NewGuid();
        var parameters = new QualifyLeadParameters(LeadId: leadId);
        var command = new QualifyLeadActionCommand(parameters, TenantId, AgentId);

        _leadsClient.ChangeLeadStatusAsync(leadId, Arg.Any<ChangeStatusClientRequest>(), Arg.Any<CancellationToken>())
            .Returns(new LeadResponse { Id = leadId, Name = "Maria Garcia", Email = "maria@example.com", Status = "Qualified" });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.QualifyLead);
        result.Message.Should().Contain("Maria Garcia");
        result.Message.Should().Contain("qualified");
        await _leadsClient.Received(1).ChangeLeadStatusAsync(
            leadId,
            Arg.Is<ChangeStatusClientRequest>(r => r.Status == "Qualified"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMissingLeadId_ShouldReturnFailure()
    {
        var parameters = new QualifyLeadParameters(LeadId: null);
        var command = new QualifyLeadActionCommand(parameters, TenantId, AgentId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Lead ID is required to qualify a lead.");
    }

    [Fact]
    public async Task Handle_WhenSdkThrows_ShouldReturnFailureGracefully()
    {
        var leadId = Guid.NewGuid();
        var parameters = new QualifyLeadParameters(LeadId: leadId);
        var command = new QualifyLeadActionCommand(parameters, TenantId, AgentId);

        _leadsClient.ChangeLeadStatusAsync(leadId, Arg.Any<ChangeStatusClientRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}
