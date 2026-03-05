// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Propely.AiApi.Application.Actions.Interfaces;
using Propely.AiApi.Domain.Actions;
using Propely.AiApi.Infrastructure.Services;

namespace Propely.AiApi.Infrastructure.AI;

/// <summary>
/// Classifies user intent using OpenAI function calling.
/// Sends the user text with tool definitions; if the model selects a tool,
/// the function name and arguments are mapped to a ClassifiedIntent.
/// </summary>
public sealed class OpenAiIntentClassifier : IIntentClassifier
{
    private readonly ChatClient? _chatClient;
    private readonly ILogger<OpenAiIntentClassifier> _logger;

    private const string SystemPrompt = """
        You are the AI assistant for Propely, a Spanish real estate management platform.
        Your role is to understand what action the real estate agent wants to perform and call the appropriate function.

        The agent may speak in Spanish, English, or other languages. Always interpret their intent regardless of language.

        You help agents manage properties, leads, contacts, appointments, and operations.
        When the user's request matches one of your available functions, call that function with the extracted parameters.
        If the request is unclear, ambiguous, or does not match any function, do NOT call any function — simply respond with a message asking for clarification.

        Important guidelines:
        - Extract as many parameters as possible from the user's text
        - For prices, convert shorthand like "250k" to 250000, "1M" to 1000000
        - For property types, normalize to the enum values (e.g., "piso" → "apartment", "chalet" → "house")
        - For operation types, normalize (e.g., "vender" → "sale", "alquilar" → "rent")
        - If the user mentions a number of rooms/bedrooms, extract it
        - If the user mentions a city or location, extract it
        """;

    public OpenAiIntentClassifier(IOptions<OpenAiOptions> options, ILogger<OpenAiIntentClassifier> logger)
    {
        _logger = logger;
        var apiKey = options.Value.ApiKey;
        var modelId = options.Value.ModelId;

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _chatClient = new ChatClient(modelId, apiKey);
        }
    }

    public async Task<ClassifiedIntent> ClassifyAsync(string text, CancellationToken ct = default)
    {
        if (_chatClient is null)
        {
            _logger.LogWarning("OpenAI API key is not configured. Returning Unknown intent.");
            return new ClassifiedIntent(ActionType.Unknown, new Dictionary<string, object?>(), 0.0);
        }

        var chatOptions = new ChatCompletionOptions();
        foreach (var tool in ToolDefinitions.All)
        {
            chatOptions.Tools.Add(tool);
        }

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(SystemPrompt),
            new UserChatMessage(text)
        };

        ChatCompletion completion;
        try
        {
            completion = await _chatClient.CompleteChatAsync(messages, chatOptions, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI API call failed during intent classification");
            throw;
        }

        // If the model selected a tool call, extract the function name and arguments
        if (completion.ToolCalls is { Count: > 0 })
        {
            var toolCall = completion.ToolCalls[0];
            var functionName = toolCall.FunctionName;
            var actionType = ToolDefinitions.ResolveActionType(functionName);

            var parameters = ParseFunctionArguments(toolCall.FunctionArguments);

            _logger.LogInformation(
                "Classified intent: {ActionType} (function: {FunctionName}) with {ParamCount} parameters",
                actionType, functionName, parameters.Count);

            return new ClassifiedIntent(
                ActionType: actionType,
                Parameters: parameters,
                Confidence: 1.0,
                RawFunctionName: functionName);
        }

        // No tool was called — the model couldn't match the input to any action
        _logger.LogInformation("No tool call in response. Returning Unknown intent for text: {TextPreview}",
            text.Length > 100 ? text[..100] + "..." : text);

        return new ClassifiedIntent(ActionType.Unknown, new Dictionary<string, object?>(), 0.0);
    }

    private Dictionary<string, object?> ParseFunctionArguments(BinaryData functionArguments)
    {
        var parameters = new Dictionary<string, object?>();

        try
        {
            using var doc = JsonDocument.Parse(functionArguments);
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                parameters[property.Name] = property.Value.ValueKind switch
                {
                    JsonValueKind.String => property.Value.GetString(),
                    JsonValueKind.Number => property.Value.TryGetInt64(out var l) ? l : property.Value.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => property.Value.GetRawText()
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse function arguments from OpenAI response");
        }

        return parameters;
    }
}
