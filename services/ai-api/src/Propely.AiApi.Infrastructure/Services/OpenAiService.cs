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
        EnsureClientConfigured();

        try
        {
            ChatCompletion completion = await _chatClient!.CompleteChatAsync(
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

    public async Task<string> GenerateWithJsonResponseAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        EnsureClientConfigured();

        try
        {
            var chatOptions = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            ChatCompletion completion = await _chatClient!.CompleteChatAsync(
                [
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userPrompt)
                ],
                chatOptions,
                cancellationToken);

            if (completion.Content != null && completion.Content.Count > 0)
            {
                return completion.Content[0].Text;
            }

            return string.Empty;
        }
        catch (Exception ex) when (ex is not AiServiceException)
        {
            throw new AiServiceException("Failed to generate JSON response from OpenAI.", ex);
        }
    }

    public async Task<string> AnalyzeImagesAsync(
        string systemPrompt,
        string[] imageUrls,
        CancellationToken cancellationToken = default)
    {
        EnsureClientConfigured();

        try
        {
            var imageParts = new List<ChatMessageContentPart>();
            foreach (var url in imageUrls)
            {
                imageParts.Add(ChatMessageContentPart.CreateImagePart(new Uri(url)));
            }

            imageParts.Add(ChatMessageContentPart.CreateTextPart(
                "Analyze the images above and extract property information as instructed."));

            var chatOptions = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            ChatCompletion completion = await _chatClient!.CompleteChatAsync(
                [
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(imageParts)
                ],
                chatOptions,
                cancellationToken);

            if (completion.Content != null && completion.Content.Count > 0)
            {
                return completion.Content[0].Text;
            }

            return string.Empty;
        }
        catch (Exception ex) when (ex is not AiServiceException)
        {
            throw new AiServiceException("Failed to analyze images with OpenAI.", ex);
        }
    }

    private void EnsureClientConfigured()
    {
        if (_chatClient == null)
        {
            throw new InvalidOperationException(
                "OpenAI API key is not configured. Set the AIAPI_OpenAi__ApiKey environment variable.");
        }
    }
}
