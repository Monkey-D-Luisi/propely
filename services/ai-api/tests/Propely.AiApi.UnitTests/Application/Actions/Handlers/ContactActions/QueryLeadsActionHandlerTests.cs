// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.AiApi.Application.Actions.Commands.ContactActions;
using Propely.AiApi.Application.Actions.Handlers.ContactActions;
using Propely.AiApi.Domain.Actions;
using Propely.ContactsApi.Client;
using Propely.ContactsApi.Client.Models;

namespace Propely.AiApi.UnitTests.Application.Actions.Handlers.ContactActions;

public sealed class QueryLeadsActionHandlerTests
{
    private readonly ILeadsApiClient _leadsClient;
    private readonly QueryLeadsActionHandler _handler;
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AgentId = Guid.NewGuid();

    public QueryLeadsActionHandlerTests()
    {
        _leadsClient = Substitute.For<ILeadsApiClient>();
        var logger = Substitute.For<ILogger<QueryLeadsActionHandler>>();
        _handler = new QueryLeadsActionHandler(_leadsClient, logger);
    }

    [Fact]
    public async Task Handle_WhenLeadsFound_ShouldReturnSuccessWithSummary()
    {
        var parameters = new Dictionary<string, object?> { ["status"] = "New" };
        var command = new QueryLeadsActionCommand(parameters, TenantId, AgentId);

        var leads = new List<LeadResponse>
        {
            new() { Id = Guid.NewGuid(), Name = "Maria Garcia", Email = "maria@example.com", Status = "New", CreatedAtUtc = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Juan Lopez", Email = "juan@example.com", Status = "New", CreatedAtUtc = DateTime.UtcNow }
        };

        _leadsClient.GetLeadsAsync(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(new LeadListResponse
            {
                Items = leads,
                PageNumber = 1,
                TotalPages = 1,
                TotalCount = 2,
                HasPreviousPage = false,
                HasNextPage = false
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.QueryLeads);
        result.Message.Should().Contain("2 leads");
        result.Message.Should().Contain("New");
    }

    [Fact]
    public async Task Handle_WhenNoLeadsFound_ShouldReturnNoResultsMessage()
    {
        var parameters = new Dictionary<string, object?> { ["status"] = "Converted" };
        var command = new QueryLeadsActionCommand(parameters, TenantId, AgentId);

        _leadsClient.GetLeadsAsync(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(new LeadListResponse
            {
                Items = [],
                PageNumber = 1,
                TotalPages = 0,
                TotalCount = 0, HasPreviousPage = false, HasNextPage = false
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ActionType.Should().Be(ActionType.QueryLeads);
        result.Message.Should().Contain("No leads found");
    }

    [Fact]
    public async Task Handle_ShouldPassFiltersToSDK()
    {
        var propertyId = Guid.NewGuid();
        var parameters = new Dictionary<string, object?>
        {
            ["status"] = "New",
            ["property_id"] = propertyId.ToString(),
            ["search"] = "Garcia"
        };
        var command = new QueryLeadsActionCommand(parameters, TenantId, AgentId);

        _leadsClient.GetLeadsAsync(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(new LeadListResponse
            {
                Items = [], TotalCount = 0, PageNumber = 1, TotalPages = 0
            });

        await _handler.Handle(command, CancellationToken.None);

        await _leadsClient.Received(1).GetLeadsAsync(
            search: "Garcia",
            status: "New",
            propertyId: propertyId,
            assignedAgentId: null,
            sortBy: "createdAt",
            sortDesc: true,
            page: 1,
            pageSize: 20,
            ct: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSdkThrows_ShouldReturnFailureGracefully()
    {
        var parameters = new Dictionary<string, object?> { ["status"] = "New" };
        var command = new QueryLeadsActionCommand(parameters, TenantId, AgentId);

        _leadsClient.GetLeadsAsync(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(),
            Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ActionType.Should().Be(ActionType.QueryLeads);
        result.Errors.Should().NotBeEmpty();
    }
}
