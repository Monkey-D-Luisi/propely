// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.ContactsApi.Application.Common.Interfaces;
using Propely.ContactsApi.Application.Leads.Commands.CreateLead;
using Propely.ContactsApi.Application.Leads.Interfaces;
using Propely.ContactsApi.Domain.Common.Exceptions;
using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.UnitTests.Application.Leads.Commands;

public class CreateLeadCommandHandlerTests
{
    private readonly ILeadRepository _leadRepository = Substitute.For<ILeadRepository>();
    private readonly ILeadReadRepository _leadReadRepository = Substitute.For<ILeadReadRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateLeadCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid PropertyId = Guid.NewGuid();

    public CreateLeadCommandHandlerTests()
    {
        _handler = new CreateLeadCommandHandler(_leadRepository, _leadReadRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_NewLead_CreatesLeadAndReturnsDto()
    {
        var command = new CreateLeadCommand
        {
            Name = "Maria Garcia",
            Email = "maria@example.com",
            PropertyId = PropertyId,
            TenantId = TenantId,
            Source = "Portal"
        };

        _leadReadRepository.ExistsByEmailAndPropertyAsync(
                Arg.Any<string>(), PropertyId, TenantId, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Maria Garcia");
        result.PropertyId.Should().Be(PropertyId);
        result.Status.Should().Be(LeadStatus.New);

        await _leadRepository.Received(1).AddAsync(Arg.Any<Lead>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateEmailAndProperty_ThrowsConflictException()
    {
        var command = new CreateLeadCommand
        {
            Name = "Maria Garcia",
            Email = "maria@example.com",
            PropertyId = PropertyId,
            TenantId = TenantId
        };

        _leadReadRepository.ExistsByEmailAndPropertyAsync(
                Arg.Any<string>(), PropertyId, TenantId, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
