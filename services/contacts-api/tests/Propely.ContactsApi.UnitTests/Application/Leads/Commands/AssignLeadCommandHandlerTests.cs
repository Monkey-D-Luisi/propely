// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Propely.ContactsApi.Application.Common.Interfaces;
using Propely.ContactsApi.Application.Leads.Commands.AssignLead;
using Propely.ContactsApi.Application.Leads.Interfaces;
using Propely.ContactsApi.Domain.Common.Exceptions;
using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.UnitTests.Application.Leads.Commands;

public class AssignLeadCommandHandlerTests
{
    private readonly ILeadRepository _leadRepository = Substitute.For<ILeadRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AssignLeadCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();

    public AssignLeadCommandHandlerTests()
    {
        _handler = new AssignLeadCommandHandler(_leadRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_LeadFound_AssignsAgentAndReturnsDto()
    {
        var lead = Lead.Create(
            name: "Maria Garcia",
            email: "maria@example.com",
            propertyId: Guid.NewGuid(),
            tenantId: TenantId);
        lead.ClearDomainEvents();

        var agentId = Guid.NewGuid();
        var command = new AssignLeadCommand(lead.Id, TenantId, agentId);

        _leadRepository.GetByIdAsync(command.LeadId, TenantId, Arg.Any<CancellationToken>())
            .Returns(lead);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AssignedAgentId.Should().Be(agentId);
        _leadRepository.Received(1).Update(lead);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LeadNotFound_ThrowsNotFoundException()
    {
        var command = new AssignLeadCommand(Guid.NewGuid(), TenantId, Guid.NewGuid());

        _leadRepository.GetByIdAsync(command.LeadId, TenantId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
