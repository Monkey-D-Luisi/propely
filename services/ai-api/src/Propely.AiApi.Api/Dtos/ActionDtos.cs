// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Api.Dtos;

/// <summary>
/// Request to execute an AI action from natural language text.
/// </summary>
public sealed record ExecuteActionRequest(string Text);

/// <summary>
/// Response containing the result of an AI action execution.
/// </summary>
public sealed record ActionResultDto(
    bool Success,
    string ActionType,
    object? Data,
    string Message,
    string[] Errors,
    double Confidence);
