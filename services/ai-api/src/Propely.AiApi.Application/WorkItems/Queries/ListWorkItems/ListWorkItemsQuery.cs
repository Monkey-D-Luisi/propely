// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.Common.Models;
using Propely.AiApi.Application.WorkItems.Dtos;
using Propely.AiApi.Domain.WorkItems;
using MediatR;

namespace Propely.AiApi.Application.WorkItems.Queries.ListWorkItems;

public record ListWorkItemsQuery(
    int Page = 1,
    int PageSize = 10,
    WorkItemStatus? Status = null,
    string? Search = null) : IRequest<PagedResult<WorkItemDto>>;
