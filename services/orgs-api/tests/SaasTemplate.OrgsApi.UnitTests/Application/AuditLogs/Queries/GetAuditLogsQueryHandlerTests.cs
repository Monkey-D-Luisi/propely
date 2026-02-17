// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.AuditLogs.Interfaces;
using SaasTemplate.OrgsApi.Application.AuditLogs.Queries.GetAuditLogs;
using SaasTemplate.OrgsApi.Domain.Common;
using FluentAssertions;
using NSubstitute;

namespace SaasTemplate.OrgsApi.UnitTests.Application.AuditLogs.Queries;

public sealed class GetAuditLogsQueryHandlerTests
{
    private readonly IAuditLogRepository _repository = Substitute.For<IAuditLogRepository>();
    private readonly GetAuditLogsQueryHandler _handler;

    public GetAuditLogsQueryHandlerTests()
    {
        _handler = new GetAuditLogsQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnPagedResults()
    {
        var logs = new List<AuditLog>
        {
            AuditLog.Create(Guid.NewGuid(), Guid.NewGuid(), "Added", "Organization", Guid.NewGuid().ToString(), "{}", null),
            AuditLog.Create(Guid.NewGuid(), null, "Modified", "User", Guid.NewGuid().ToString(), null, "corr-1"),
        };

        _repository.GetPagedAsync(1, 50, null, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((logs, 2));

        var query = new GetAuditLogsQuery(1, 50, null, null, null, null, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithActionFilter_ShouldPassFilterToRepository()
    {
        _repository.GetPagedAsync(1, 50, null, null, null, "Added", null, null, Arg.Any<CancellationToken>())
            .Returns((new List<AuditLog>(), 0));

        var query = new GetAuditLogsQuery(1, 50, null, null, null, "Added", null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        await _repository.Received(1).GetPagedAsync(1, 50, null, null, null, "Added", null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDateRange_ShouldPassDatesToRepository()
    {
        var dateFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var dateTo = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        _repository.GetPagedAsync(1, 50, dateFrom, dateTo, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<AuditLog>(), 0));

        var query = new GetAuditLogsQuery(1, 50, dateFrom, dateTo, null, null, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        await _repository.Received(1).GetPagedAsync(1, 50, dateFrom, dateTo, null, null, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMapDtoFieldsCorrectly()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var entityId = Guid.NewGuid().ToString();
        var log = AuditLog.Create(userId, orgId, "Deleted", "Membership", entityId, "{\"role\":\"admin\"}", "corr-123");

        _repository.GetPagedAsync(1, 50, null, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<AuditLog> { log }, 1));

        var query = new GetAuditLogsQuery(1, 50, null, null, null, null, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        var dto = result.Items.Should().ContainSingle().Subject;
        dto.UserId.Should().Be(userId);
        dto.OrganizationId.Should().Be(orgId);
        dto.Action.Should().Be("Deleted");
        dto.EntityType.Should().Be("Membership");
        dto.EntityId.Should().Be(entityId);
        dto.Changes.Should().Be("{\"role\":\"admin\"}");
        dto.CorrelationId.Should().Be("corr-123");
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPageInfo()
    {
        _repository.GetPagedAsync(2, 10, null, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<AuditLog>(), 25));

        var query = new GetAuditLogsQuery(2, 10, null, null, null, null, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.PageNumber.Should().Be(2);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }
}
