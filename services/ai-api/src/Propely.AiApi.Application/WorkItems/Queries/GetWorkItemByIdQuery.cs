// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.WorkItems.Dtos;
using MediatR;

namespace Propely.AiApi.Application.WorkItems.Queries;

/// <summary>
/// Query to retrieve a WorkItem by its unique identifier.
/// </summary>
public sealed record GetWorkItemByIdQuery(Guid Id) : IRequest<WorkItemDto?>;
