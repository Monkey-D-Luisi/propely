// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Application.Common.Models;
using SaasTemplate.AiApi.Application.WorkItems.Dtos;
using SaasTemplate.AiApi.Application.WorkItems.Interfaces;
using SaasTemplate.AiApi.Domain.WorkItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SaasTemplate.AiApi.Infrastructure.Persistence.Repositories;

/// <summary>
/// Read repository implementation for WorkItem queries.
/// Uses the read model (WorkItemsRead) for optimized query performance.
/// Falls back to write model if read model not yet projected (eventual consistency).
/// </summary>
public sealed class WorkItemReadRepository : IWorkItemReadRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<WorkItemReadRepository> _logger;

    public WorkItemReadRepository(AppDbContext context, ILogger<WorkItemReadRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<WorkItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Try read model first (optimized for queries)
        var readModel = await _context.WorkItemsRead
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (readModel is not null)
        {
            var status = ParseWorkItemStatus(readModel.Status, readModel.Id);
            var priority = ParseWorkItemPriority(readModel.Priority, readModel.Id);
            var type = ParseWorkItemType(readModel.Type, readModel.Id);
            var effort = ParseWorkItemEffort(readModel.EstimatedEffort, readModel.Id);

            return new WorkItemDto(
                readModel.Id,
                readModel.OrgId,
                readModel.Title,
                readModel.Description,
                status,
                priority,
                type,
                readModel.DueDateUtc,
                effort,
                readModel.CreatedAtUtc,
                readModel.LastProjectedAtUtc);
        }

        // Fallback to write model if read model not yet projected (eventual consistency)
        var writeModel = await _context.WorkItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (writeModel is null)
        {
            return null;
        }

        _logger.LogDebug(
            "Read model not yet projected for WorkItem {Id}. Falling back to write model.",
            id);

        return new WorkItemDto(
            writeModel.Id,
            writeModel.OrgId,
            writeModel.Title,
            writeModel.Description,
            writeModel.Status,
            writeModel.Priority,
            writeModel.Type,
            writeModel.DueDateUtc,
            writeModel.EstimatedEffort,
            writeModel.CreatedAtUtc,
            writeModel.UpdatedAtUtc);
    }

    /// <inheritdoc />
    public async Task<PagedResult<WorkItemDto>> ListAsync(
        int page,
        int pageSize,
        WorkItemStatus? status,
        string? search,
        CancellationToken cancellationToken)
    {
        var query = _context.WorkItemsRead.AsNoTracking();

        if (status.HasValue)
        {
            var statusString = status.Value.ToString();
            query = query.Where(x => x.Status == statusString);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var escapedSearch = EscapeLikeWildcards(search);
            query = query.Where(x => EF.Functions.ILike(x.Title, $"%{escapedSearch}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var readModels = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = readModels.Select(x =>
        {
            var itemStatus = ParseWorkItemStatus(x.Status, x.Id);
            var itemPriority = ParseWorkItemPriority(x.Priority, x.Id);
            var itemType = ParseWorkItemType(x.Type, x.Id);
            var itemEffort = ParseWorkItemEffort(x.EstimatedEffort, x.Id);

            return new WorkItemDto(
                x.Id,
                x.OrgId,
                x.Title,
                x.Description,
                itemStatus,
                itemPriority,
                itemType,
                x.DueDateUtc,
                itemEffort,
                x.CreatedAtUtc,
                x.LastProjectedAtUtc);
        });

        return new PagedResult<WorkItemDto>(items, totalCount, page, pageSize);
    }

    /// <summary>
    /// Parses a status string from the read model into a <see cref="WorkItemStatus"/> enum.
    /// Logs a warning and defaults to <see cref="WorkItemStatus.Pending"/> if parsing fails.
    /// </summary>
    private WorkItemStatus ParseWorkItemStatus(string statusString, Guid workItemId)
    {
        if (Enum.TryParse<WorkItemStatus>(statusString, out var status))
        {
            return status;
        }

        _logger.LogWarning(
            "Failed to parse WorkItemStatus '{Status}' for WorkItem {Id}. Defaulting to Pending.",
            statusString,
            workItemId);

        return WorkItemStatus.Pending;
    }

    /// <summary>
    /// Parses a priority string from the read model into a <see cref="WorkItemPriority"/> enum.
    /// Returns null if the string is null or parsing fails.
    /// </summary>
    private WorkItemPriority? ParseWorkItemPriority(string? priorityString, Guid workItemId)
    {
        if (string.IsNullOrEmpty(priorityString))
        {
            return null;
        }

        if (Enum.TryParse<WorkItemPriority>(priorityString, out var priority))
        {
            return priority;
        }

        _logger.LogWarning(
            "Failed to parse WorkItemPriority '{Priority}' for WorkItem {Id}. Returning null.",
            priorityString,
            workItemId);

        return null;
    }

    /// <summary>
    /// Parses a type string from the read model into a <see cref="WorkItemType"/> enum.
    /// Returns null if the string is null or parsing fails.
    /// </summary>
    private WorkItemType? ParseWorkItemType(string? typeString, Guid workItemId)
    {
        if (string.IsNullOrEmpty(typeString))
        {
            return null;
        }

        if (Enum.TryParse<WorkItemType>(typeString, out var type))
        {
            return type;
        }

        _logger.LogWarning(
            "Failed to parse WorkItemType '{Type}' for WorkItem {Id}. Returning null.",
            typeString,
            workItemId);

        return null;
    }

    /// <summary>
    /// Parses an effort string from the read model into a <see cref="WorkItemEffort"/> enum.
    /// Returns null if the string is null or parsing fails.
    /// </summary>
    private WorkItemEffort? ParseWorkItemEffort(string? effortString, Guid workItemId)
    {
        if (string.IsNullOrEmpty(effortString))
        {
            return null;
        }

        if (Enum.TryParse<WorkItemEffort>(effortString, out var effort))
        {
            return effort;
        }

        _logger.LogWarning(
            "Failed to parse WorkItemEffort '{Effort}' for WorkItem {Id}. Returning null.",
            effortString,
            workItemId);

        return null;
    }

    /// <summary>
    /// Escapes PostgreSQL LIKE/ILIKE wildcard characters (%, _, \) in user input
    /// to prevent unintended pattern matching.
    /// </summary>
    private static string EscapeLikeWildcards(string input)
    {
        return input
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_");
    }
}
