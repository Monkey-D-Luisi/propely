// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Application.WorkItems.Dtos;
using SaasTemplate.AiApi.Domain.WorkItems;

namespace SaasTemplate.AiApi.Api.Dtos;

/// <summary>
/// Response DTO for work item operations.
/// </summary>
public sealed record WorkItemResponse(
    Guid Id,
    Guid OrgId,
    string Title,
    string? Description,
    string Status,
    string? Priority,
    string? Type,
    DateTime? DueDateUtc,
    string? EstimatedEffort,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc)
{
    /// <summary>
    /// Creates a response from the application DTO.
    /// </summary>
    public static WorkItemResponse FromDto(WorkItemDto dto) =>
        new(
            dto.Id,
            dto.OrgId,
            dto.Title,
            dto.Description,
            dto.Status.ToString(),
            dto.Priority?.ToString(),
            dto.Type?.ToString(),
            dto.DueDateUtc,
            dto.EstimatedEffort?.ToString(),
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc);

    /// <summary>
    /// Creates a minimal response for POST 201 Created.
    /// </summary>
    public static WorkItemResponse Created(
        Guid id,
        Guid orgId,
        string title,
        string? description,
        WorkItemStatus status,
        WorkItemPriority? priority,
        WorkItemType? type,
        DateTime? dueDateUtc,
        WorkItemEffort? estimatedEffort,
        DateTime createdAtUtc,
        DateTime updatedAtUtc) =>
        new(id, orgId, title, description, status.ToString(),
            priority?.ToString(), type?.ToString(), dueDateUtc, estimatedEffort?.ToString(),
            createdAtUtc, updatedAtUtc);
}
