// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.AiApi.Api.Controllers;
using Propely.AiApi.Api.Dtos;
using Propely.AiApi.Application.Actions.Commands.ExecuteAction;
using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Domain.Actions;

using DomainActionResult = Propely.AiApi.Domain.Actions.ActionResult;

namespace Propely.AiApi.UnitTests.Api.Controllers;

public sealed class VoiceControllerTests
{
    private readonly IVoiceTranscriptionService _transcriptionService;
    private readonly IMediator _mediator;
    private readonly VoiceController _controller;

    private static readonly Guid TestUserId = Guid.NewGuid();
    private static readonly Guid TestOrgId = Guid.NewGuid();

    public VoiceControllerTests()
    {
        _transcriptionService = Substitute.For<IVoiceTranscriptionService>();
        _mediator = Substitute.For<IMediator>();
        _controller = new VoiceController(_transcriptionService, _mediator);
        SetupAuthenticatedUser(_controller);
    }

    #region Transcribe Endpoint Tests

    [Fact]
    public async Task Transcribe_WhenAudioFileIsNull_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.Transcribe(null!, null, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeAssignableTo<ProblemDetails>();
        var details = (ValidationProblemDetails)objectResult.Value!;
        details.Errors.Should().ContainKey("audio");
    }

    [Fact]
    public async Task Transcribe_WhenAudioFileIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var file = CreateMockFormFile("test.wav", "audio/wav", 0);

        // Act
        var result = await _controller.Transcribe(file, null, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeAssignableTo<ProblemDetails>();
        var details = (ValidationProblemDetails)objectResult.Value!;
        details.Errors.Should().ContainKey("audio");
    }

    [Fact]
    public async Task Transcribe_WhenFileSizeExceedsLimit_ShouldReturnBadRequest()
    {
        // Arrange — 26 MB file
        var file = CreateMockFormFile("test.wav", "audio/wav", 26 * 1024 * 1024);

        // Act
        var result = await _controller.Transcribe(file, null, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeAssignableTo<ProblemDetails>();
        var details = (ValidationProblemDetails)objectResult.Value!;
        details.Errors.Should().ContainKey("audio");
        details.Errors["audio"].Should().Contain(e => e.Contains("25 MB"));
    }

    [Fact]
    public async Task Transcribe_WhenUnsupportedContentType_ShouldReturnBadRequest()
    {
        // Arrange
        var file = CreateMockFormFile("test.txt", "text/plain", 1024);

        // Act
        var result = await _controller.Transcribe(file, null, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeAssignableTo<ProblemDetails>();
        var details = (ValidationProblemDetails)objectResult.Value!;
        details.Errors.Should().ContainKey("audio");
        details.Errors["audio"].Should().Contain(e => e.Contains("Unsupported"));
    }

    [Theory]
    [InlineData("audio/webm", ".webm")]
    [InlineData("audio/wav", ".wav")]
    [InlineData("audio/mpeg", ".mp3")]
    [InlineData("audio/mp4", ".m4a")]
    [InlineData("audio/ogg", ".ogg")]
    [InlineData("audio/flac", ".flac")]
    public async Task Transcribe_WhenSupportedContentType_ShouldTranscribeSuccessfully(string contentType, string extension)
    {
        // Arrange
        var fileName = $"test{extension}";
        var file = CreateMockFormFile(fileName, contentType, 1024);
        _transcriptionService.TranscribeAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Hello world", "en", 3200));

        // Act
        var result = await _controller.Transcribe(file, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var dto = okResult.Value.Should().BeOfType<TranscriptionResultDto>().Subject;
        dto.Text.Should().Be("Hello world");
        dto.Language.Should().Be("en");
        dto.DurationMs.Should().Be(3200);
    }

    [Fact]
    public async Task Transcribe_WhenValidAudioWithLanguageHint_ShouldPassLanguageToService()
    {
        // Arrange
        var file = CreateMockFormFile("test.wav", "audio/wav", 1024);
        _transcriptionService.TranscribeAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Is("es"), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Hola mundo", "es", 2500));

        // Act
        var result = await _controller.Transcribe(file, "es", CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var dto = okResult.Value.Should().BeOfType<TranscriptionResultDto>().Subject;
        dto.Text.Should().Be("Hola mundo");
        dto.Language.Should().Be("es");
    }

    [Fact]
    public async Task Transcribe_WhenUserNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        SetupUnauthenticatedUser(_controller);
        var file = CreateMockFormFile("test.wav", "audio/wav", 1024);

        // Act
        var result = await _controller.Transcribe(file, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Transcribe_WhenUserHasNoOrg_ShouldReturnForbid()
    {
        // Arrange
        SetupUserWithoutOrg(_controller);
        var file = CreateMockFormFile("test.wav", "audio/wav", 1024);

        // Act
        var result = await _controller.Transcribe(file, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Transcribe_WhenFileHasSupportedExtensionButUnknownContentType_ShouldSucceed()
    {
        // Arrange — extension-based validation should pass even with generic content type
        var file = CreateMockFormFile("test.webm", "application/octet-stream", 1024);
        // Note: "application/octet-stream" is NOT in SupportedContentTypes,
        // but ".webm" IS in SupportedExtensions
        _transcriptionService.TranscribeAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Test", "en", 1000));

        // Act
        var result = await _controller.Transcribe(file, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Transcribe_WhenFileSizeIsExactlyAtLimit_ShouldSucceed()
    {
        // Arrange — exactly 25 MB
        var file = CreateMockFormFile("test.wav", "audio/wav", VoiceController.MaxFileSizeBytes);
        _transcriptionService.TranscribeAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Test", "en", 1000));

        // Act
        var result = await _controller.Transcribe(file, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Execute Endpoint Tests

    [Fact]
    public async Task Execute_WhenAudioFileIsNull_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.Execute(null!, null, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeAssignableTo<ProblemDetails>();
    }

    [Fact]
    public async Task Execute_WhenUnsupportedFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var file = CreateMockFormFile("test.pdf", "application/pdf", 1024);

        // Act
        var result = await _controller.Execute(file, null, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeAssignableTo<ProblemDetails>();
    }

    [Fact]
    public async Task Execute_WhenFileTooLarge_ShouldReturnBadRequest()
    {
        // Arrange
        var file = CreateMockFormFile("test.wav", "audio/wav", 30 * 1024 * 1024);

        // Act
        var result = await _controller.Execute(file, null, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeAssignableTo<ProblemDetails>();
    }

    [Fact]
    public async Task Execute_WhenTranscriptionReturnsEmptyText_ShouldReturnFailureWithMessage()
    {
        // Arrange
        var file = CreateMockFormFile("test.wav", "audio/wav", 1024);
        _transcriptionService.TranscribeAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("", "en", 500));

        // Act
        var result = await _controller.Execute(file, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var dto = okResult.Value.Should().BeOfType<VoiceExecuteResultDto>().Subject;
        dto.TranscribedText.Should().BeEmpty();
        dto.Action.Success.Should().BeFalse();
        dto.Action.Message.Should().Contain("No speech detected");
    }

    [Fact]
    public async Task Execute_WhenTranscriptionSucceeds_ShouldSendCommandToMediator()
    {
        // Arrange
        var file = CreateMockFormFile("test.wav", "audio/wav", 1024);
        _transcriptionService.TranscribeAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Create a property in Madrid", "es", 3200));

        var actionResult = DomainActionResult.Ok(
            new { Id = Guid.NewGuid() },
            "Property created successfully",
            ActionType.CreateProperty,
            0.95);

        _mediator.Send(Arg.Any<ExecuteActionCommand>(), Arg.Any<CancellationToken>())
            .Returns(actionResult);

        // Act
        var result = await _controller.Execute(file, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var dto = okResult.Value.Should().BeOfType<VoiceExecuteResultDto>().Subject;
        dto.TranscribedText.Should().Be("Create a property in Madrid");
        dto.Language.Should().Be("es");
        dto.DurationMs.Should().Be(3200);
        dto.Action.Success.Should().BeTrue();
        dto.Action.ActionType.Should().Be("CreateProperty");
        dto.Action.Message.Should().Be("Property created successfully");
        dto.Action.Confidence.Should().Be(0.95);

        await _mediator.Received(1).Send(
            Arg.Is<ExecuteActionCommand>(c =>
                c.Text == "Create a property in Madrid" &&
                c.TenantId == TestOrgId &&
                c.AgentId == TestUserId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_WhenActionFails_ShouldReturnActionFailureWithTranscription()
    {
        // Arrange
        var file = CreateMockFormFile("test.mp3", "audio/mpeg", 1024);
        _transcriptionService.TranscribeAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("Do something unknown", "en", 1500));

        var actionResult = DomainActionResult.Fail(
            ["Could not understand the request."],
            ActionType.Unknown,
            "I didn't understand that command.");

        _mediator.Send(Arg.Any<ExecuteActionCommand>(), Arg.Any<CancellationToken>())
            .Returns(actionResult);

        // Act
        var result = await _controller.Execute(file, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var dto = okResult.Value.Should().BeOfType<VoiceExecuteResultDto>().Subject;
        dto.TranscribedText.Should().Be("Do something unknown");
        dto.Action.Success.Should().BeFalse();
        dto.Action.ActionType.Should().Be("Unknown");
    }

    [Fact]
    public async Task Execute_WhenUserNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        SetupUnauthenticatedUser(_controller);
        var file = CreateMockFormFile("test.wav", "audio/wav", 1024);

        // Act
        var result = await _controller.Execute(file, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Execute_WhenUserHasNoOrg_ShouldReturnForbid()
    {
        // Arrange
        SetupUserWithoutOrg(_controller);
        var file = CreateMockFormFile("test.wav", "audio/wav", 1024);

        // Act
        var result = await _controller.Execute(file, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Execute_WhenLanguageHintProvided_ShouldPassToTranscriptionService()
    {
        // Arrange
        var file = CreateMockFormFile("test.m4a", "audio/mp4", 1024);
        _transcriptionService.TranscribeAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Is("en"), Arg.Any<CancellationToken>())
            .Returns(new TranscriptionResult("List all properties", "en", 2000));

        _mediator.Send(Arg.Any<ExecuteActionCommand>(), Arg.Any<CancellationToken>())
            .Returns(DomainActionResult.Ok(null, "Listed properties", ActionType.QueryProperties));

        // Act
        var result = await _controller.Execute(file, "en", CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        await _transcriptionService.Received(1).TranscribeAsync(
            Arg.Any<Stream>(),
            Arg.Any<string>(),
            "en",
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Validation Edge Cases

    [Fact]
    public void SupportedContentTypes_ShouldContainAllExpectedTypes()
    {
        // Assert
        VoiceController.SupportedContentTypes.Should().Contain("audio/webm");
        VoiceController.SupportedContentTypes.Should().Contain("audio/wav");
        VoiceController.SupportedContentTypes.Should().Contain("audio/mpeg");
        VoiceController.SupportedContentTypes.Should().Contain("audio/mp3");
        VoiceController.SupportedContentTypes.Should().Contain("audio/mp4");
        VoiceController.SupportedContentTypes.Should().Contain("audio/ogg");
        VoiceController.SupportedContentTypes.Should().Contain("audio/flac");
    }

    [Fact]
    public void SupportedExtensions_ShouldContainAllExpectedExtensions()
    {
        // Assert
        VoiceController.SupportedExtensions.Should().Contain(".webm");
        VoiceController.SupportedExtensions.Should().Contain(".wav");
        VoiceController.SupportedExtensions.Should().Contain(".mp3");
        VoiceController.SupportedExtensions.Should().Contain(".m4a");
        VoiceController.SupportedExtensions.Should().Contain(".ogg");
        VoiceController.SupportedExtensions.Should().Contain(".flac");
    }

    [Fact]
    public void MaxFileSizeBytes_ShouldBe25MB()
    {
        // Assert
        VoiceController.MaxFileSizeBytes.Should().Be(25 * 1024 * 1024);
    }

    #endregion

    #region Helpers

    private static void SetupAuthenticatedUser(ControllerBase controller)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId.ToString()),
            new Claim("org_id", TestOrgId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private static void SetupUnauthenticatedUser(ControllerBase controller)
    {
        var identity = new ClaimsIdentity(); // No claims
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private static void SetupUserWithoutOrg(ControllerBase controller)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId.ToString())
            // No org_id claim
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private static IFormFile CreateMockFormFile(string fileName, string contentType, long length)
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns(fileName);
        file.ContentType.Returns(contentType);
        file.Length.Returns(length);
        file.OpenReadStream().Returns(new MemoryStream(new byte[Math.Min(length, 1024)]));
        return file;
    }

    #endregion
}
