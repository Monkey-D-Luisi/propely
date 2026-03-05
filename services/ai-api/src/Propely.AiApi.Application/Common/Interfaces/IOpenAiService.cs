// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Threading;
using System.Threading.Tasks;

namespace Propely.AiApi.Application.Common.Interfaces;

public interface IOpenAiService
{
    Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a system prompt and user prompt to OpenAI with JSON response format enabled,
    /// returning the raw JSON string response.
    /// </summary>
    Task<string> GenerateWithJsonResponseAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends images (via URLs) to OpenAI vision API with a system prompt,
    /// returning the raw text/JSON response describing the images.
    /// </summary>
    Task<string> AnalyzeImagesAsync(
        string systemPrompt,
        string[] imageUrls,
        CancellationToken cancellationToken = default);
}
