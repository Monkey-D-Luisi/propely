// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Client.Dtos;
using Refit;

namespace Propely.AiApi.Client;

/// <summary>
/// Refit-based typed HTTP client for the AI API action execution endpoints.
/// </summary>
[Headers("Content-Type: application/json")]
public interface IActionApi
{
    /// <summary>
    /// Executes an AI action from natural language text input.
    /// The text is classified into an intent, routed to the appropriate handler, and executed.
    /// </summary>
    /// <param name="request">The action request containing natural language text.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the action execution.</returns>
    [Post("/v1/actions/execute")]
    Task<ActionResultDto> ExecuteActionAsync([Body] ExecuteActionRequest request, CancellationToken ct = default);
}
