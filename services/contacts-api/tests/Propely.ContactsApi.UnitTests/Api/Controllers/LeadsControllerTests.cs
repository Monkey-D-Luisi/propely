// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Propely.ContactsApi.Api.Controllers;
using Propely.ContactsApi.Api.Dtos;
using Propely.ContactsApi.Application.Common.Models;
using Propely.ContactsApi.Application.Contacts.Dtos;
using Propely.ContactsApi.Application.Leads.Commands.AssignLead;
using Propely.ContactsApi.Application.Leads.Commands.ChangeLeadStatus;
using Propely.ContactsApi.Application.Leads.Commands.ConvertLead;
using Propely.ContactsApi.Application.Leads.Commands.CreateLead;
using Propely.ContactsApi.Application.Leads.Commands.DeleteLead;
using Propely.ContactsApi.Application.Leads.Dtos;
using Propely.ContactsApi.Application.Leads.Queries.GetLeadById;
using Propely.ContactsApi.Application.Leads.Queries.ListLeads;
using Propely.ContactsApi.Domain.Contacts;
using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.UnitTests.Api.Controllers;

public class LeadsControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly LeadsController _controller;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid LeadId = Guid.NewGuid();

    public LeadsControllerTests()
    {
        _controller = new LeadsController(_mediator);
        SetAuthenticatedUser(UserId, TenantId);
    }

    private void SetAuthenticatedUser(Guid? userId, Guid? tenantId)
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
            claims.Add(new Claim("sub", userId.Value.ToString()));
        if (tenantId.HasValue)
            claims.Add(new Claim("org_id", tenantId.Value.ToString()));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            }
        };
    }

    private static LeadDto MakeLeadDto() => new()
    {
        Id = LeadId,
        Name = "Pedro Gómez",
        Email = "pedro@test.com",
        PropertyId = Guid.NewGuid(),
        TenantId = TenantId,
        Status = LeadStatus.New,
        CreatedAtUtc = DateTime.UtcNow
    };

    // --- Create ---

    [Fact]
    public async Task Create_WithValidClaims_Returns201()
    {
        _mediator.Send(Arg.Any<CreateLeadCommand>(), Arg.Any<CancellationToken>()).Returns(MakeLeadDto());
        var request = new CreateLeadRequest { Name = "Pedro", Email = "pedro@test.com", PropertyId = Guid.NewGuid() };

        var result = (ObjectResult)await _controller.Create(request, CancellationToken.None);

        result.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_MissingUserId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(null, TenantId);

        var result = await _controller.Create(
            new CreateLeadRequest { Name = "Pedro", Email = "pedro@test.com", PropertyId = Guid.NewGuid() },
            CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Create_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.Create(
            new CreateLeadRequest { Name = "Pedro", Email = "pedro@test.com", PropertyId = Guid.NewGuid() },
            CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_LeadFound_ReturnsOk()
    {
        var dto = MakeLeadDto();
        _mediator.Send(Arg.Any<GetLeadByIdQuery>(), Arg.Any<CancellationToken>()).Returns(dto);

        var result = (OkObjectResult)await _controller.GetById(LeadId, CancellationToken.None);

        result.StatusCode.Should().Be(200);
        result.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_LeadNotFound_ReturnsNotFound()
    {
        _mediator.Send(Arg.Any<GetLeadByIdQuery>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _controller.GetById(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.GetById(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- List ---

    [Fact]
    public async Task List_WithValidClaims_ReturnsOk()
    {
        var paged = new PagedResult<LeadListItemDto>([], 0, 1, 20);
        _mediator.Send(Arg.Any<ListLeadsQuery>(), Arg.Any<CancellationToken>()).Returns(paged);

        var result = (OkObjectResult)await _controller.List(null, null, null, null, null, false, 1, 20, CancellationToken.None);

        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task List_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.List(null, null, null, null, null, false, 1, 20, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task List_PageSizeOutOfRange_ClampsValues()
    {
        var paged = new PagedResult<LeadListItemDto>([], 0, 1, 100);
        _mediator.Send(Arg.Any<ListLeadsQuery>(), Arg.Any<CancellationToken>()).Returns(paged);

        var result = (OkObjectResult)await _controller.List(null, null, null, null, null, false, 0, 500, CancellationToken.None);

        result.StatusCode.Should().Be(200);
        await _mediator.Received(1).Send(Arg.Is<ListLeadsQuery>(q => q.Page == 1 && q.PageSize == 100), Arg.Any<CancellationToken>());
    }

    // --- Delete ---

    [Fact]
    public async Task Delete_WithValidClaims_ReturnsNoContent()
    {
        _mediator.Send(Arg.Any<DeleteLeadCommand>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(Unit.Value));

        var result = await _controller.Delete(LeadId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.Delete(LeadId, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- Assign ---

    [Fact]
    public async Task Assign_WithValidClaims_ReturnsOk()
    {
        _mediator.Send(Arg.Any<AssignLeadCommand>(), Arg.Any<CancellationToken>()).Returns(MakeLeadDto());

        var result = (OkObjectResult)await _controller.Assign(
            LeadId, new AssignLeadRequest { AgentId = Guid.NewGuid() }, CancellationToken.None);

        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Assign_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.Assign(LeadId, new AssignLeadRequest { AgentId = Guid.NewGuid() }, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- ChangeStatus ---

    [Fact]
    public async Task ChangeStatus_WithValidClaims_ReturnsOk()
    {
        _mediator.Send(Arg.Any<ChangeLeadStatusCommand>(), Arg.Any<CancellationToken>()).Returns(MakeLeadDto());

        var result = (OkObjectResult)await _controller.ChangeStatus(
            LeadId, new ChangeLeadStatusRequest { Status = LeadStatus.Contacted }, CancellationToken.None);

        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ChangeStatus_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.ChangeStatus(
            LeadId, new ChangeLeadStatusRequest { Status = LeadStatus.Contacted }, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- Convert ---

    [Fact]
    public async Task Convert_WithValidClaims_ReturnsOk()
    {
        var response = new ConvertLeadResponse
        {
            Lead = MakeLeadDto(),
            Contact = new ContactDto { Id = Guid.NewGuid(), FirstName = "Pedro", LastName = "Gómez", Email = "pedro@test.com", TenantId = TenantId, Roles = [], CreatedAtUtc = DateTime.UtcNow },
            WasNewContact = true
        };
        _mediator.Send(Arg.Any<ConvertLeadCommand>(), Arg.Any<CancellationToken>()).Returns(response);

        var result = (OkObjectResult)await _controller.Convert(
            LeadId, new ConvertLeadRequest { Role = ContactRole.Buyer }, CancellationToken.None);

        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Convert_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.Convert(LeadId, new ConvertLeadRequest { Role = ContactRole.Buyer }, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }
}
