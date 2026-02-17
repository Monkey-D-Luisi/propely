// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Application.Common.Interfaces;
using SaasTemplate.AiApi.Application.Common.Telemetry;
using SaasTemplate.AiApi.Application.WorkItems;
using SaasTemplate.AiApi.Application.WorkItems.Dtos;
using SaasTemplate.AiApi.Application.WorkItems.Interfaces;
using SaasTemplate.AiApi.Application.WorkItems.Queries;
using SaasTemplate.AiApi.Domain.WorkItems;
using FluentAssertions;
using NSubstitute;

namespace SaasTemplate.AiApi.UnitTests.Application.WorkItems;

public sealed class GetWorkItemByIdQueryHandlerTests
{
    private readonly IWorkItemReadRepository _readRepository;
    private readonly ICacheService _cacheService;
    private readonly ICacheSettings _cacheSettings;
    private readonly GetWorkItemByIdQueryHandler _handler;

    public GetWorkItemByIdQueryHandlerTests()
    {
        _readRepository = Substitute.For<IWorkItemReadRepository>();
        _cacheService = Substitute.For<ICacheService>();
        _cacheSettings = Substitute.For<ICacheSettings>();
        _cacheSettings.DefaultTtlMinutes.Returns(5);
        _handler = new GetWorkItemByIdQueryHandler(
            _readRepository, 
            _cacheService, 
            _cacheSettings,
            new WorkItemMetrics());
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ShouldReturnCachedDto()
    {
        // Arrange
        var workItemId = Guid.NewGuid();
        var cachedDto = new WorkItemDto(
            workItemId,
            Guid.NewGuid(),
            "Cached Title",
            "Cached Description",
            WorkItemStatus.Pending,
            null,
            null,
            null,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow);
        var query = new GetWorkItemByIdQuery(workItemId);
        var cacheKey = WorkItemCacheKeys.GetById(workItemId);

        _cacheService
            .GetAsync<WorkItemDto>(cacheKey, Arg.Any<CancellationToken>())
            .Returns(cachedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(workItemId);
        result.Title.Should().Be("Cached Title");

        // Should not call read repository on cache hit
        await _readRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ShouldQueryReadModelAndCache()
    {
        // Arrange
        var workItemId = Guid.NewGuid();
        var dto = new WorkItemDto(
            workItemId,
            Guid.NewGuid(),
            "Test Title",
            "Test Description",
            WorkItemStatus.Pending,
            null,
            null,
            null,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow);
        var query = new GetWorkItemByIdQuery(workItemId);
        var cacheKey = WorkItemCacheKeys.GetById(workItemId);

        _cacheService
            .GetAsync<WorkItemDto>(cacheKey, Arg.Any<CancellationToken>())
            .Returns((WorkItemDto?)null);

        _readRepository
            .GetByIdAsync(workItemId, Arg.Any<CancellationToken>())
            .Returns(dto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(workItemId);
        result.Title.Should().Be("Test Title");
        result.Description.Should().Be("Test Description");
        result.Status.Should().Be(WorkItemStatus.Pending);

        // Should call read repository
        await _readRepository.Received(1).GetByIdAsync(workItemId, Arg.Any<CancellationToken>());

        // Should cache the result
        await _cacheService.Received(1).SetAsync(
            cacheKey,
            Arg.Any<WorkItemDto>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWorkItemDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var query = new GetWorkItemByIdQuery(nonExistentId);
        var cacheKey = WorkItemCacheKeys.GetById(nonExistentId);

        _cacheService
            .GetAsync<WorkItemDto>(cacheKey, Arg.Any<CancellationToken>())
            .Returns((WorkItemDto?)null);

        _readRepository
            .GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((WorkItemDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        // Should not cache null results
        await _cacheService.DidNotReceive().SetAsync(
            Arg.Any<string>(),
            Arg.Any<WorkItemDto>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallReadRepositoryWithCorrectId()
    {
        // Arrange
        var queryId = Guid.NewGuid();
        var query = new GetWorkItemByIdQuery(queryId);

        _cacheService
            .GetAsync<WorkItemDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((WorkItemDto?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _readRepository.Received(1).GetByIdAsync(queryId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassItToReadRepository()
    {
        // Arrange
        var queryId = Guid.NewGuid();
        var query = new GetWorkItemByIdQuery(queryId);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _cacheService
            .GetAsync<WorkItemDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((WorkItemDto?)null);

        // Act
        await _handler.Handle(query, token);

        // Assert
        await _readRepository.Received(1).GetByIdAsync(queryId, token);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllPropertiesFromReadRepository()
    {
        // Arrange
        var workItemId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;
        var dto = new WorkItemDto(
            workItemId,
            Guid.NewGuid(),
            "Title",
            "Description",
            WorkItemStatus.Pending,
            null,
            null,
            null,
            null,
            createdAt,
            updatedAt);
        var query = new GetWorkItemByIdQuery(workItemId);

        _cacheService
            .GetAsync<WorkItemDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((WorkItemDto?)null);

        _readRepository
            .GetByIdAsync(workItemId, Arg.Any<CancellationToken>())
            .Returns(dto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(workItemId);
        result.Title.Should().Be("Title");
        result.Description.Should().Be("Description");
        result.Status.Should().Be(WorkItemStatus.Pending);
        result.CreatedAtUtc.Should().Be(createdAt);
        result.UpdatedAtUtc.Should().Be(updatedAt);
    }

    [Fact]
    public async Task Handle_WhenWorkItemHasNoDescription_ShouldReturnDtoWithNullDescription()
    {
        // Arrange
        var workItemId = Guid.NewGuid();
        var dto = new WorkItemDto(
            workItemId,
            Guid.NewGuid(),
            "Title Only",
            null,
            WorkItemStatus.Pending,
            null,
            null,
            null,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow);
        var query = new GetWorkItemByIdQuery(workItemId);

        _cacheService
            .GetAsync<WorkItemDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((WorkItemDto?)null);

        _readRepository
            .GetByIdAsync(workItemId, Arg.Any<CancellationToken>())
            .Returns(dto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Description.Should().BeNull();
    }

    [Fact]
    public void GetCacheKey_ShouldReturnCorrectFormat()
    {
        // Arrange
        var workItemId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        // Act
        var cacheKey = WorkItemCacheKeys.GetById(workItemId);

        // Assert
        cacheKey.Should().Be("workitem:12345678-1234-1234-1234-123456789012");
    }
}
