// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.AiApi.Application.WorkItems.Commands.ParseWorkItem;

/// <summary>
/// Command to parse natural language text into structured work item fields using AI.
/// </summary>
public sealed record ParseWorkItemCommand(string Text) : IRequest<ParseWorkItemResult>;

/// <summary>
/// Result of AI work item parsing.
/// </summary>
public sealed record ParseWorkItemResult(
    string Title,
    string? Description,
    string Status,
    string? Priority,
    string? Type,
    string? DueDateUtc,
    string? EstimatedEffort,
    double Confidence);
