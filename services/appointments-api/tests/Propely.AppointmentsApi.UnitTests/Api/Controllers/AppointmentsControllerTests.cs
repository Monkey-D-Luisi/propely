// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Propely.AppointmentsApi.Api.Controllers;
using Propely.AppointmentsApi.Api.Dtos;
using Propely.AppointmentsApi.Application.Appointments.Commands.CancelAppointment;
using Propely.AppointmentsApi.Application.Appointments.Commands.CompleteAppointment;
using Propely.AppointmentsApi.Application.Appointments.Commands.ConfirmAppointment;
using Propely.AppointmentsApi.Application.Appointments.Commands.CreateAppointment;
using Propely.AppointmentsApi.Application.Appointments.Commands.DeleteAppointment;
using Propely.AppointmentsApi.Application.Appointments.Commands.MarkNoShowAppointment;
using Propely.AppointmentsApi.Application.Appointments.Commands.UpdateAppointment;
using Propely.AppointmentsApi.Application.Appointments.Dtos;
using Propely.AppointmentsApi.Application.Appointments.Queries.GetAppointmentById;
using Propely.AppointmentsApi.Application.Appointments.Queries.ListAppointments;
using Propely.AppointmentsApi.Application.Common.Models;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.UnitTests.Api.Controllers;

public class AppointmentsControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ILogger<AppointmentsController> _logger = Substitute.For<ILogger<AppointmentsController>>();
    private readonly AppointmentsController _controller;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AppointmentId = Guid.NewGuid();

    public AppointmentsControllerTests()
    {
        _controller = new AppointmentsController(_mediator, _logger);
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

    private static AppointmentDto MakeAppointmentDto() => new()
    {
        Id = AppointmentId,
        Title = "Property Viewing - Calle Mayor",
        Type = AppointmentType.PropertyViewing,
        Status = AppointmentStatus.Scheduled,
        StartTimeUtc = DateTime.UtcNow.AddDays(1),
        EndTimeUtc = DateTime.UtcNow.AddDays(1).AddHours(1),
        AgentId = UserId,
        TenantId = TenantId,
        PropertyId = Guid.NewGuid(),
        CreatedAtUtc = DateTime.UtcNow
    };

    // --- Create ---

    [Fact]
    public async Task Create_WithValidClaims_Returns201()
    {
        _mediator.Send(Arg.Any<CreateAppointmentCommand>(), Arg.Any<CancellationToken>()).Returns(MakeAppointmentDto());
        var request = new CreateAppointmentApiRequest
        {
            Title = "Property Viewing",
            Type = AppointmentType.PropertyViewing,
            StartTimeUtc = DateTime.UtcNow.AddDays(1),
            EndTimeUtc = DateTime.UtcNow.AddDays(1).AddHours(1),
            PropertyId = Guid.NewGuid()
        };

        var result = (ObjectResult)await _controller.Create(request, CancellationToken.None);

        result.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_MissingUserId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(null, TenantId);

        var result = await _controller.Create(
            new CreateAppointmentApiRequest
            {
                Title = "Test",
                Type = AppointmentType.Generic,
                StartTimeUtc = DateTime.UtcNow.AddDays(1),
                EndTimeUtc = DateTime.UtcNow.AddDays(1).AddHours(1)
            },
            CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Create_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.Create(
            new CreateAppointmentApiRequest
            {
                Title = "Test",
                Type = AppointmentType.Generic,
                StartTimeUtc = DateTime.UtcNow.AddDays(1),
                EndTimeUtc = DateTime.UtcNow.AddDays(1).AddHours(1)
            },
            CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_AppointmentFound_ReturnsOk()
    {
        var dto = MakeAppointmentDto();
        _mediator.Send(Arg.Any<GetAppointmentByIdQuery>(), Arg.Any<CancellationToken>()).Returns(dto);

        var result = (OkObjectResult)await _controller.GetById(AppointmentId, CancellationToken.None);

        result.StatusCode.Should().Be(200);
        result.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_AppointmentNotFound_ReturnsNotFound()
    {
        _mediator.Send(Arg.Any<GetAppointmentByIdQuery>(), Arg.Any<CancellationToken>()).ReturnsNull();

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
        var paged = new PagedResult<AppointmentListItemDto>([], 0, 1, 20);
        _mediator.Send(Arg.Any<ListAppointmentsQuery>(), Arg.Any<CancellationToken>()).Returns(paged);

        var result = (OkObjectResult)await _controller.List(
            null, null, null, null, null, null, null, null, null, false, 1, 20, CancellationToken.None);

        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task List_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.List(
            null, null, null, null, null, null, null, null, null, false, 1, 20, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task List_PageSizeOutOfRange_ClampsValues()
    {
        var paged = new PagedResult<AppointmentListItemDto>([], 0, 1, 100);
        _mediator.Send(Arg.Any<ListAppointmentsQuery>(), Arg.Any<CancellationToken>()).Returns(paged);

        var result = (OkObjectResult)await _controller.List(
            null, null, null, null, null, null, null, null, null, false, 0, 500, CancellationToken.None);

        result.StatusCode.Should().Be(200);
        await _mediator.Received(1).Send(
            Arg.Is<ListAppointmentsQuery>(q => q.Page == 1 && q.PageSize == 100),
            Arg.Any<CancellationToken>());
    }

    // --- Update ---

    [Fact]
    public async Task Update_WithValidClaims_ReturnsOk()
    {
        _mediator.Send(Arg.Any<UpdateAppointmentCommand>(), Arg.Any<CancellationToken>()).Returns(MakeAppointmentDto());

        var request = new UpdateAppointmentApiRequest
        {
            Title = "Updated Title",
            StartTimeUtc = DateTime.UtcNow.AddDays(2),
            EndTimeUtc = DateTime.UtcNow.AddDays(2).AddHours(1)
        };

        var result = (OkObjectResult)await _controller.Update(AppointmentId, request, CancellationToken.None);

        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Update_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.Update(
            AppointmentId,
            new UpdateAppointmentApiRequest
            {
                Title = "Test",
                StartTimeUtc = DateTime.UtcNow.AddDays(1),
                EndTimeUtc = DateTime.UtcNow.AddDays(1).AddHours(1)
            },
            CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- Delete ---

    [Fact]
    public async Task Delete_WithValidClaims_ReturnsNoContent()
    {
        _mediator.Send(Arg.Any<DeleteAppointmentCommand>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(Unit.Value));

        var result = await _controller.Delete(AppointmentId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.Delete(AppointmentId, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- Confirm ---

    [Fact]
    public async Task Confirm_WithValidClaims_ReturnsOk()
    {
        _mediator.Send(Arg.Any<ConfirmAppointmentCommand>(), Arg.Any<CancellationToken>()).Returns(MakeAppointmentDto());

        var result = (OkObjectResult)await _controller.Confirm(AppointmentId, CancellationToken.None);

        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Confirm_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.Confirm(AppointmentId, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- Complete ---

    [Fact]
    public async Task Complete_WithValidClaims_ReturnsOk()
    {
        _mediator.Send(Arg.Any<CompleteAppointmentCommand>(), Arg.Any<CancellationToken>()).Returns(MakeAppointmentDto());

        var result = (OkObjectResult)await _controller.Complete(
            AppointmentId, new CompleteAppointmentApiRequest { Notes = "All done" }, CancellationToken.None);

        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Complete_WithNullBody_ReturnsOk()
    {
        _mediator.Send(Arg.Any<CompleteAppointmentCommand>(), Arg.Any<CancellationToken>()).Returns(MakeAppointmentDto());

        var result = (OkObjectResult)await _controller.Complete(AppointmentId, null, CancellationToken.None);

        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Complete_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.Complete(
            AppointmentId, new CompleteAppointmentApiRequest(), CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- Cancel ---

    [Fact]
    public async Task Cancel_WithValidClaims_ReturnsOk()
    {
        _mediator.Send(Arg.Any<CancelAppointmentCommand>(), Arg.Any<CancellationToken>()).Returns(MakeAppointmentDto());

        var result = (OkObjectResult)await _controller.Cancel(
            AppointmentId, new CancelAppointmentApiRequest { Reason = "Client unavailable" }, CancellationToken.None);

        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Cancel_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.Cancel(
            AppointmentId, new CancelAppointmentApiRequest { Reason = "Test" }, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- MarkNoShow ---

    [Fact]
    public async Task MarkNoShow_WithValidClaims_ReturnsOk()
    {
        _mediator.Send(Arg.Any<MarkNoShowAppointmentCommand>(), Arg.Any<CancellationToken>()).Returns(MakeAppointmentDto());

        var result = (OkObjectResult)await _controller.MarkNoShow(AppointmentId, CancellationToken.None);

        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task MarkNoShow_MissingTenantId_ReturnsUnauthorized()
    {
        SetAuthenticatedUser(UserId, null);

        var result = await _controller.MarkNoShow(AppointmentId, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }
}
