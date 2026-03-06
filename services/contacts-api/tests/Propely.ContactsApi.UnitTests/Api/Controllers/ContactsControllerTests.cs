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
using Propely.ContactsApi.Application.Contacts.Commands.CreateContact;
using Propely.ContactsApi.Application.Contacts.Commands.DeleteContact;
using Propely.ContactsApi.Application.Contacts.Commands.UpdateContact;
using Propely.ContactsApi.Application.Contacts.Dtos;
using Propely.ContactsApi.Application.Contacts.Queries.GetContactById;
using Propely.ContactsApi.Application.Contacts.Queries.ListContacts;
using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.UnitTests.Api.Controllers;

public class ContactsControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ContactsController _controller;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();

    public ContactsControllerTests()
    {
        _controller = new ContactsController(_mediator);
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

    private static ContactDto MakeContactDto() => new()
    {
        Id = Guid.NewGuid(),
        FirstName = "Ana",
        LastName = "Lopez",
        Email = "ana@test.com",
        TenantId = TenantId,
        Roles = [ContactRole.Buyer],
        CreatedAtUtc = DateTime.UtcNow
    };

    // --- Create ---

    [Fact]
    public async Task Create_WithValidClaims_Returns201()
    {
        _mediator.Send(Arg.Any<CreateContactCommand>(), Arg.Any<CancellationToken>()).Returns(MakeContactDto());

        var result = (ObjectResult)await _controller.Create(
            new CreateContactRequest { FirstName = "Ana", LastName = "Lopez", Email = "ana@test.com", Roles = [ContactRole.Buyer] },
            CancellationToken.None);

        result.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_MissingUserId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(null, TenantId);

        var result = await _controller.Create(
            new CreateContactRequest { FirstName = "Ana", LastName = "Lopez", Email = "ana@test.com" },
            CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Create_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.Create(
            new CreateContactRequest { FirstName = "Ana", LastName = "Lopez", Email = "ana@test.com" },
            CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_ContactFound_ReturnsOk()
    {
        var dto = MakeContactDto();
        _mediator.Send(Arg.Any<GetContactByIdQuery>(), Arg.Any<CancellationToken>()).Returns(dto);

        var result = (OkObjectResult)await _controller.GetById(dto.Id, CancellationToken.None);

        result.StatusCode.Should().Be(200);
        result.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_ContactNotFound_ReturnsNotFound()
    {
        _mediator.Send(Arg.Any<GetContactByIdQuery>(), Arg.Any<CancellationToken>()).ReturnsNull();

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
        var paged = new PagedResult<ContactListItemDto>([], 0, 1, 20);
        _mediator.Send(Arg.Any<ListContactsQuery>(), Arg.Any<CancellationToken>()).Returns(paged);

        var result = (OkObjectResult)await _controller.List(null, null, null, false, 1, 20, CancellationToken.None);

        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task List_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.List(null, null, null, false, 1, 20, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task List_PageSizeOutOfRange_ClampsValues()
    {
        var paged = new PagedResult<ContactListItemDto>([], 0, 1, 100);
        _mediator.Send(Arg.Any<ListContactsQuery>(), Arg.Any<CancellationToken>()).Returns(paged);

        var result = (OkObjectResult)await _controller.List(null, null, null, false, 0, 200, CancellationToken.None);

        result.StatusCode.Should().Be(200);
        await _mediator.Received(1).Send(Arg.Is<ListContactsQuery>(q => q.Page == 1 && q.PageSize == 100), Arg.Any<CancellationToken>());
    }

    // --- Update ---

    [Fact]
    public async Task Update_WithValidClaims_ReturnsOk()
    {
        var dto = MakeContactDto();
        _mediator.Send(Arg.Any<UpdateContactCommand>(), Arg.Any<CancellationToken>()).Returns(dto);

        var result = (OkObjectResult)await _controller.Update(dto.Id, new UpdateContactRequest { FirstName = "Ana" }, CancellationToken.None);

        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Update_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.Update(Guid.NewGuid(), new UpdateContactRequest(), CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- Delete ---

    [Fact]
    public async Task Delete_WithValidClaims_ReturnsNoContent()
    {
        _mediator.Send(Arg.Any<DeleteContactCommand>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(Unit.Value));

        var result = await _controller.Delete(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.Delete(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }
}
