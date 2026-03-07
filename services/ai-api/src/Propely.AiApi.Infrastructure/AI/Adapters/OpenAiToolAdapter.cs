// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using OpenAI.Chat;
using Propely.AiApi.Application.Actions.Tools;

namespace Propely.AiApi.Infrastructure.AI.Adapters;

/// <summary>
/// Converts provider-neutral <see cref="ToolSchema"/> instances into OpenAI <see cref="ChatTool"/> objects.
/// </summary>
public sealed class OpenAiToolAdapter : IToolAdapter
{
    public IReadOnlyList<object> ConvertAll(IReadOnlyList<ToolSchema> schemas)
    {
        var tools = new List<object>(schemas.Count);
        foreach (var schema in schemas)
        {
            tools.Add(ChatTool.CreateFunctionTool(
                functionName: schema.Name,
                functionDescription: schema.Description,
                functionParameters: BinaryData.FromString(schema.ParametersJsonSchema)));
        }
        return tools;
    }
}
