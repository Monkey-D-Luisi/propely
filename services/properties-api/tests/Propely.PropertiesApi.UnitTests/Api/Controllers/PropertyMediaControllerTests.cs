// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Propely.PropertiesApi.Api.Controllers;
using Propely.PropertiesApi.Api.Dtos;
using Propely.PropertiesApi.Application.Properties.Commands.DeleteMedia;
using Propely.PropertiesApi.Application.Properties.Commands.ReorderMedia;
using Propely.PropertiesApi.Application.Properties.Commands.UploadMedia;
using Propely.PropertiesApi.Application.Properties.Dtos;
using Propely.PropertiesApi.Application.Properties.Queries.ListPropertyMedia;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Api.Controllers;

public class PropertyMediaControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly PropertyMediaController _controller;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid PropertyId = Guid.NewGuid();

    public PropertyMediaControllerTests()
    {
        _controller = new PropertyMediaController(_mediator);
        SetupControllerContext(TenantId);
    }

    private void SetupControllerContext(Guid? orgId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };

        if (orgId.HasValue)
            claims.Add(new Claim("org_id", orgId.Value.ToString()));

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task Upload_ValidFile_ReturnsCreated()
    {
        var uploadResult = new UploadMediaResult(Guid.NewGuid(), "path/file.jpg", "path/thumb.jpg");
        _mediator.Send(Arg.Any<UploadMediaCommand>(), Arg.Any<CancellationToken>())
            .Returns(uploadResult);

        var file = Substitute.For<IFormFile>();
        file.FileName.Returns("photo.jpg");
        file.ContentType.Returns("image/jpeg");
        file.Length.Returns(1024L);
        file.OpenReadStream().Returns(new MemoryStream([1, 2, 3]));

        var result = await _controller.Upload(PropertyId, file);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Upload_NullFile_ReturnsBadRequest()
    {
        var result = await _controller.Upload(PropertyId, null!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Upload_NoTenantId_ReturnsUnauthorized()
    {
        SetupControllerContext(null);

        var file = Substitute.For<IFormFile>();
        file.Length.Returns(1024L);

        var result = await _controller.Upload(PropertyId, file);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task List_ValidRequest_ReturnsOk()
    {
        var mediaList = new List<PropertyMediaDto>
        {
            new() { Id = Guid.NewGuid(), FileName = "photo.jpg", Url = "https://url" }
        };
        _mediator.Send(Arg.Any<ListPropertyMediaQuery>(), Arg.Any<CancellationToken>())
            .Returns(mediaList as IReadOnlyList<PropertyMediaDto>);

        var result = await _controller.List(PropertyId, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(mediaList);
    }

    [Fact]
    public async Task List_NoTenantId_ReturnsUnauthorized()
    {
        SetupControllerContext(null);

        var result = await _controller.List(PropertyId, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Delete_ValidRequest_ReturnsNoContent()
    {
        var mediaId = Guid.NewGuid();

        var result = await _controller.Delete(PropertyId, mediaId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        await _mediator.Received(1).Send(Arg.Is<DeleteMediaCommand>(c =>
            c.MediaId == mediaId && c.PropertyId == PropertyId && c.TenantId == TenantId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_NoTenantId_ReturnsUnauthorized()
    {
        SetupControllerContext(null);

        var result = await _controller.Delete(PropertyId, Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Reorder_ValidRequest_ReturnsOk()
    {
        var request = new ReorderRequest
        {
            Items =
            [
                new ReorderItemRequest { MediaId = Guid.NewGuid(), DisplayOrder = 0 },
                new ReorderItemRequest { MediaId = Guid.NewGuid(), DisplayOrder = 1 }
            ]
        };

        var result = await _controller.Reorder(PropertyId, request, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await _mediator.Received(1).Send(Arg.Any<ReorderMediaCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reorder_NoTenantId_ReturnsUnauthorized()
    {
        SetupControllerContext(null);
        var request = new ReorderRequest { Items = [] };

        var result = await _controller.Reorder(PropertyId, request, CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }
}
