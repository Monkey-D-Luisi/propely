// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Diagnostics;
using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Application.Common.Telemetry;
using Propely.AiApi.Application.WorkItems.Dtos;
using Propely.AiApi.Application.WorkItems.Interfaces;
using MediatR;
using Propely.AiApi.Domain.WorkItems;

namespace Propely.AiApi.Application.WorkItems.Queries;

/// <summary>
/// Handles the retrieval of a WorkItem by its ID with caching support.
/// Uses the read model for optimized query performance (CQRS pattern).
/// </summary>
public sealed class GetWorkItemByIdQueryHandler : IRequestHandler<GetWorkItemByIdQuery, WorkItemDto?>
{
    private readonly IWorkItemReadRepository _readRepository;
    private readonly ICacheService _cacheService;
    private readonly TimeSpan _cacheTtl;
    private readonly WorkItemMetrics _metrics;

    public GetWorkItemByIdQueryHandler(
        IWorkItemReadRepository readRepository,
        ICacheService cacheService,
        ICacheSettings cacheSettings,
        WorkItemMetrics metrics)
    {
        _readRepository = readRepository;
        _cacheService = cacheService;
        _cacheTtl = TimeSpan.FromMinutes(cacheSettings.DefaultTtlMinutes);
        _metrics = metrics;
    }

    /// <summary>
    /// Handles the GetWorkItemByIdQuery using cache-aside pattern.
    /// </summary>
    /// <remarks>
    /// 1. Check cache first with key "workitem:{id}"
    /// 2. On cache hit, return cached value
    /// 3. On cache miss, query read model, cache result, return
    /// </remarks>
    /// <param name="request">The query containing the work item ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The WorkItemDto if found; otherwise, null.</returns>
    public async Task<WorkItemDto?> Handle(
        GetWorkItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var cacheKey = WorkItemCacheKeys.GetById(request.Id);

            // Try cache first
            var cached = await _cacheService.GetAsync<WorkItemDto>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }

            // Cache miss - query read model
            var dto = await _readRepository.GetByIdAsync(request.Id, cancellationToken);

            if (dto is null || dto.Status == WorkItemStatus.Deleted)
            {
                return null;
            }

            // Cache the result with configured TTL
            await _cacheService.SetAsync(cacheKey, dto, _cacheTtl, cancellationToken);

            return dto;
        }
        finally
        {
            stopwatch.Stop();
            _metrics.RecordQueryDuration(stopwatch.Elapsed.TotalSeconds);
        }
    }

}
