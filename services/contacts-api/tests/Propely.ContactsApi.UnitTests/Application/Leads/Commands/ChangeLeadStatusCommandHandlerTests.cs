// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Propely.ContactsApi.Application.Common.Interfaces;
using Propely.ContactsApi.Application.Leads.Commands.ChangeLeadStatus;
using Propely.ContactsApi.Application.Leads.Interfaces;
using Propely.ContactsApi.Domain.Common.Exceptions;
using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.UnitTests.Application.Leads.Commands;

public class ChangeLeadStatusCommandHandlerTests
{
    private readonly ILeadRepository _leadRepository = Substitute.For<ILeadRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ChangeLeadStatusCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();

    public ChangeLeadStatusCommandHandlerTests()
    {
        _handler = new ChangeLeadStatusCommandHandler(_leadRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidTransition_ChangesStatusAndReturnsDto()
    {
        var lead = Lead.Create(
            name: "Maria Garcia",
            email: "maria@example.com",
            propertyId: Guid.NewGuid(),
            tenantId: TenantId);
        // Lead is in New status
        lead.ClearDomainEvents();

        var command = new ChangeLeadStatusCommand(lead.Id, TenantId, LeadStatus.Contacted);

        _leadRepository.GetByIdAsync(command.LeadId, TenantId, Arg.Any<CancellationToken>())
            .Returns(lead);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(LeadStatus.Contacted);
        _leadRepository.Received(1).Update(lead);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_LeadNotFound_ThrowsNotFoundException()
    {
        var command = new ChangeLeadStatusCommand(Guid.NewGuid(), TenantId, LeadStatus.Contacted);

        _leadRepository.GetByIdAsync(command.LeadId, TenantId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
