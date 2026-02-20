// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Threading;
using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Domain.Common.Exceptions;
using OpenAI.Chat;
using Microsoft.Extensions.Options;

namespace Propely.AiApi.Infrastructure.Services;

public class OpenAiService : IOpenAiService
{
    private readonly ChatClient? _chatClient;

    public OpenAiService(IOptions<OpenAiOptions> options)
    {
        var apiKey = options.Value.ApiKey;
        var modelId = options.Value.ModelId;

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _chatClient = new ChatClient(modelId, apiKey);
        }
    }

    public async Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (_chatClient == null)
        {
            throw new InvalidOperationException(
                "OpenAI API key is not configured. Set the AIAPI_OpenAi__ApiKey environment variable.");
        }

        try
        {
            ChatCompletion completion = await _chatClient.CompleteChatAsync(
                [new UserChatMessage(prompt)],
                options: null,
                cancellationToken: cancellationToken);

            if (completion.Content != null && completion.Content.Count > 0)
            {
                return completion.Content[0].Text;
            }

            return string.Empty;
        }
        catch (Exception ex) when (ex is not AiServiceException)
        {
            throw new AiServiceException("Failed to generate text from OpenAI.", ex);
        }
    }
}
