// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.AuditLogs.Interfaces;
using Propely.OrgsApi.Application.AuditLogs.Queries.ExportAuditLogs;
using Propely.OrgsApi.Domain.Common;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.AuditLogs.Queries;

public sealed class ExportAuditLogsQueryHandlerTests
{
    private readonly IAuditLogRepository _repository = Substitute.For<IAuditLogRepository>();
    private readonly ExportAuditLogsQueryHandler _handler;

    public ExportAuditLogsQueryHandlerTests()
    {
        _handler = new ExportAuditLogsQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllLogs()
    {
        var logs = new List<AuditLog>
        {
            AuditLog.Create(Guid.NewGuid(), null, "Added", "Organization", "1", null, null),
            AuditLog.Create(Guid.NewGuid(), null, "Modified", "User", "2", "{}", null),
            AuditLog.Create(null, null, "Deleted", "Membership", "3", null, "corr-1"),
        };

        _repository.GetAllFilteredAsync(null, null, null, null, null, null, ExportAuditLogsQueryHandler.MaxExportRows, Arg.Any<CancellationToken>())
            .Returns(logs);

        var query = new ExportAuditLogsQuery(null, null, null, null, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithFilters_ShouldPassFiltersToRepository()
    {
        var userId = Guid.NewGuid();
        var dateFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        _repository.GetAllFilteredAsync(dateFrom, null, userId, "Added", "Organization", null, ExportAuditLogsQueryHandler.MaxExportRows, Arg.Any<CancellationToken>())
            .Returns(new List<AuditLog>());

        var query = new ExportAuditLogsQuery(dateFrom, null, userId, "Added", "Organization", null);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
        await _repository.Received(1).GetAllFilteredAsync(dateFrom, null, userId, "Added", "Organization", null, ExportAuditLogsQueryHandler.MaxExportRows, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMapDtoFieldsCorrectly()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var log = AuditLog.Create(userId, orgId, "Modified", "FeatureFlag", "flagId", "{\"isEnabled\":true}", "corr-42");

        _repository.GetAllFilteredAsync(null, null, null, null, null, null, ExportAuditLogsQueryHandler.MaxExportRows, Arg.Any<CancellationToken>())
            .Returns(new List<AuditLog> { log });

        var query = new ExportAuditLogsQuery(null, null, null, null, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        var dto = result.Should().ContainSingle().Subject;
        dto.UserId.Should().Be(userId);
        dto.OrganizationId.Should().Be(orgId);
        dto.Action.Should().Be("Modified");
        dto.EntityType.Should().Be("FeatureFlag");
        dto.EntityId.Should().Be("flagId");
        dto.Changes.Should().Be("{\"isEnabled\":true}");
        dto.CorrelationId.Should().Be("corr-42");
    }
}
