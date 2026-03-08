// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.ContactsApi.Application.Leads.Interfaces;
using Propely.ContactsApi.Application.Leads.Queries.CountLeadsByStatus;
using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.UnitTests.Application.Leads.Queries;

public class CountLeadsByStatusQueryHandlerTests
{
    private readonly ILeadReadRepository _leadReadRepository = Substitute.For<ILeadReadRepository>();
    private readonly CountLeadsByStatusQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();

    public CountLeadsByStatusQueryHandlerTests()
    {
        _handler = new CountLeadsByStatusQueryHandler(_leadReadRepository);
    }

    [Fact]
    public async Task Handle_ReturnsCountsWithStringKeys()
    {
        var counts = new Dictionary<LeadStatus, int>
        {
            { LeadStatus.New, 5 },
            { LeadStatus.Contacted, 3 },
            { LeadStatus.Qualified, 2 },
            { LeadStatus.Converted, 1 }
        };

        _leadReadRepository.CountByStatusAsync(TenantId, Arg.Any<CancellationToken>())
            .Returns(counts);

        var query = new CountLeadsByStatusQuery(TenantId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(4);
        result["New"].Should().Be(5);
        result["Contacted"].Should().Be(3);
        result["Qualified"].Should().Be(2);
        result["Converted"].Should().Be(1);
    }

    [Fact]
    public async Task Handle_EmptyResults_ReturnsEmptyDictionary()
    {
        _leadReadRepository.CountByStatusAsync(TenantId, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<LeadStatus, int>());

        var query = new CountLeadsByStatusQuery(TenantId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PassesTenantIdToRepository()
    {
        _leadReadRepository.CountByStatusAsync(TenantId, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<LeadStatus, int>());

        var query = new CountLeadsByStatusQuery(TenantId);
        await _handler.Handle(query, CancellationToken.None);

        await _leadReadRepository.Received(1).CountByStatusAsync(TenantId, Arg.Any<CancellationToken>());
    }
}
