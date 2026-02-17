// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Application.Common.Models;
using SaasTemplate.AiApi.Application.WorkItems.Dtos;
using SaasTemplate.AiApi.Domain.WorkItems;
using MediatR;

namespace SaasTemplate.AiApi.Application.WorkItems.Queries.ListWorkItems;

public record ListWorkItemsQuery(
    int Page = 1,
    int PageSize = 10,
    WorkItemStatus? Status = null,
    string? Search = null) : IRequest<PagedResult<WorkItemDto>>;
