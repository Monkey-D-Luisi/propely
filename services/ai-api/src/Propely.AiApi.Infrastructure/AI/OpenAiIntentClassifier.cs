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

        The agent may speak in Spanish, English, or a mix of both. Always interpret their intent regardless of language.

        You help agents manage properties, leads, contacts, appointments, and operations.
        When the user's request matches one of your available functions, call that function with the extracted parameters.
        If the request is unclear, ambiguous, or does not match any function, do NOT call any function — simply respond with a message asking for clarification.

        ## Spanish Real Estate Vocabulary

        Property types (normalize to English enum):
        - piso/apartamento → apartment | ático/atico → penthouse | bajo → apartment
        - dúplex/duplex → duplex | estudio/loft → studio
        - adosado/pareado → house | chalet/chalé/casa → house
        - villa/finca/cortijo/masía → villa
        - local/local comercial/oficina/nave industrial/edificio → commercial
        - solar/terreno/parcela → land | garaje/plaza de garaje → garage | trastero → storage

        Operation types (normalize to English enum):
        - venta/vender/compra/comprar → sale
        - alquiler/alquilar/arrendar/arriendo → rent
        - alquiler vacacional/alquiler temporal → rent
        - traspaso/traspasar → transfer
        - alquiler con opción a compra → rent

        Features:
        - piscina=pool, jardín=garden, terraza=terrace, balcón=balcony
        - ascensor=elevator, calefacción=heating, aire acondicionado=air_conditioning
        - amueblado=furnished, reformado=renovated, a estrenar=new_build, luminoso=bright

        Area terms: m² construidos=built area, m² útiles=usable area, m² de parcela=plot area
        Financial terms: comunidad=HOA fees, IBI=property tax, catastro=land registry
        Energy: certificado energético=energy certificate (A-G)

        ## Important Guidelines

        - Extract as many parameters as possible from the user's text
        - For prices, convert shorthand like "250k" to 250000, "1M" to 1000000
        - For property types, always normalize to the enum values listed above
        - For operation types, always normalize to the enum values listed above
        - If the user mentions a number of rooms/bedrooms (habitaciones, dormitorios), extract it
        - If the user mentions a city or location, extract it
        - For contact roles: comprador=Buyer, vendedor=Seller, inquilino=Tenant, propietario=Landlord

        ## Examples

        User: "Crea un piso de 3 habitaciones en Málaga por 250k en venta"
        → call create_property(property_type="apartment", bedrooms=3, city="Málaga", price=250000, operation_type="sale")

        User: "Busca áticos en alquiler en Barcelona por menos de 1500 al mes"
        → call query_properties(property_type="penthouse", operation_type="rent", city="Barcelona", max_price=1500)

        User: "Agenda una visita para la propiedad X mañana a las 10"
        → call book_viewing(property_id="X", start_time="<tomorrow 10:00 ISO>")

        User: "Create a lead for Juan García, interested in property ABC"
        → call create_lead(name="Juan García", property_id="ABC")

        User: "dame los chalets en venta en Marbella de más de 500k"
        → call query_properties(property_type="house", operation_type="sale", city="Marbella", min_price=500000)
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
