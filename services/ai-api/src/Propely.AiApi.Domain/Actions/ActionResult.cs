// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Domain.Actions;

/// <summary>
/// Generic result of an AI action execution with typed data payload.
/// </summary>
public sealed record ActionResult<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string Message { get; init; } = string.Empty;
    public string[] Errors { get; init; } = [];
    public ActionType ActionType { get; init; }
    public double Confidence { get; init; }

    public static ActionResult<T> Ok(T data, string message, ActionType type, double confidence = 1.0)
        => new() { Success = true, Data = data, Message = message, ActionType = type, Confidence = confidence };

    public static ActionResult<T> Fail(string[] errors, ActionType type, string message = "")
        => new() { Success = false, Errors = errors, ActionType = type, Message = message };
}

/// <summary>
/// Non-generic result of an AI action execution with object data payload.
/// </summary>
public sealed record ActionResult
{
    public bool Success { get; init; }
    public object? Data { get; init; }
    public string Message { get; init; } = string.Empty;
    public string[] Errors { get; init; } = [];
    public ActionType ActionType { get; init; }
    public double Confidence { get; init; }

    public static ActionResult Ok(object? data, string message, ActionType type, double confidence = 1.0)
        => new() { Success = true, Data = data, Message = message, ActionType = type, Confidence = confidence };

    public static ActionResult Fail(string[] errors, ActionType type, string message = "")
        => new() { Success = false, Errors = errors, ActionType = type, Message = message };
}
