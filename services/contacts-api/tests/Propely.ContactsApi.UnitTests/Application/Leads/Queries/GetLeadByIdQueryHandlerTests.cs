// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Propely.ContactsApi.Application.Leads.Interfaces;
using Propely.ContactsApi.Application.Leads.Queries.GetLeadById;
using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.UnitTests.Application.Leads.Queries;

public class GetLeadByIdQueryHandlerTests
{
    private readonly ILeadReadRepository _leadReadRepository = Substitute.For<ILeadReadRepository>();
    private readonly GetLeadByIdQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();

    public GetLeadByIdQueryHandlerTests()
    {
        _handler = new GetLeadByIdQueryHandler(_leadReadRepository);
    }

    [Fact]
    public async Task Handle_LeadExists_ReturnsDto()
    {
        var lead = Lead.Create(
            name: "Maria Garcia",
            email: "maria@example.com",
            propertyId: Guid.NewGuid(),
            tenantId: TenantId,
            source: "Portal");

        var query = new GetLeadByIdQuery(lead.Id, TenantId);

        _leadReadRepository.GetByIdAsync(query.LeadId, TenantId, Arg.Any<CancellationToken>())
            .Returns(lead);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(lead.Id);
        result.Name.Should().Be("Maria Garcia");
        result.Email.Should().Be("maria@example.com");
    }

    [Fact]
    public async Task Handle_LeadNotFound_ReturnsNull()
    {
        var query = new GetLeadByIdQuery(Guid.NewGuid(), TenantId);

        _leadReadRepository.GetByIdAsync(query.LeadId, TenantId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
