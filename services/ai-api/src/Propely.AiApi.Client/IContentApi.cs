// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Client.Dtos;
using Refit;

namespace Propely.AiApi.Client;

/// <summary>
/// Refit-based typed HTTP client for the AI API content generation endpoints.
/// Wraps action execution with content-specific convenience methods for
/// text extraction, photo extraction, and copy generation.
/// </summary>
[Headers("Content-Type: application/json")]
public interface IContentApi
{
    /// <summary>
    /// Extracts structured property data from unstructured text via AI.
    /// This is a convenience wrapper around the action execute endpoint
    /// with text extraction intent.
    /// </summary>
    /// <param name="request">The action request containing the text to extract data from.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The action result containing extracted property data.</returns>
    [Post("/v1/actions/execute")]
    Task<ActionResultDto> ExtractFromTextAsync([Body] ExecuteActionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Generates marketing copy for a property listing via AI.
    /// This is a convenience wrapper around the action execute endpoint
    /// with copy generation intent.
    /// </summary>
    /// <param name="request">The action request containing the property data and generation parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The action result containing generated copy in multiple languages.</returns>
    [Post("/v1/actions/execute")]
    Task<ActionResultDto> GenerateCopyAsync([Body] ExecuteActionRequest request, CancellationToken ct = default);
}
