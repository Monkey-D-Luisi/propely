// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using OpenAI.Chat;
using Propely.AiApi.Application.Actions.Tools;
using Propely.AiApi.Infrastructure.AI.Adapters;

namespace Propely.AiApi.UnitTests.Infrastructure.AI.Adapters;

public sealed class OpenAiToolAdapterTests
{
    private readonly ToolSchemaRegistry _registry = new();
    private readonly OpenAiToolAdapter _adapter = new();

    [Fact]
    public void ConvertAll_ShouldReturnSameCountAsRegistry()
    {
        var tools = _adapter.ConvertAll(_registry.All);
        tools.Should().HaveCount(20);
    }

    [Fact]
    public void ConvertAll_ShouldReturnChatToolInstances()
    {
        var tools = _adapter.ConvertAll(_registry.All);
        tools.Should().AllBeAssignableTo<ChatTool>();
    }

    [Fact]
    public void ConvertAll_ShouldPreserveFunctionNames()
    {
        var tools = _adapter.ConvertAll(_registry.All);
        var chatTools = tools.Cast<ChatTool>().ToList();

        var expectedNames = _registry.All.Select(s => s.Name).ToList();
        var actualNames = chatTools.Select(t => t.FunctionName).ToList();

        actualNames.Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
    public void ConvertAll_ShouldPreserveFunctionDescriptions()
    {
        var tools = _adapter.ConvertAll(_registry.All);
        var chatTools = tools.Cast<ChatTool>().ToList();

        for (var i = 0; i < _registry.All.Count; i++)
        {
            chatTools[i].FunctionDescription.Should().Be(
                _registry.All[i].Description,
                because: $"tool '{_registry.All[i].Name}' description must be preserved");
        }
    }

    [Fact]
    public void ConvertAll_CreateProperty_ShouldHaveCorrectFunctionName()
    {
        var tools = _adapter.ConvertAll(_registry.All).Cast<ChatTool>().ToList();
        var createProperty = tools.First(t => t.FunctionName == "create_property");
        createProperty.FunctionName.Should().Be("create_property");
    }

    [Fact]
    public void ConvertAll_QueryProperties_ShouldHaveCorrectFunctionName()
    {
        var tools = _adapter.ConvertAll(_registry.All).Cast<ChatTool>().ToList();
        var queryProperties = tools.First(t => t.FunctionName == "query_properties");
        queryProperties.FunctionName.Should().Be("query_properties");
    }

    [Fact]
    public void ConvertAll_WithEmptyList_ShouldReturnEmpty()
    {
        var tools = _adapter.ConvertAll([]);
        tools.Should().BeEmpty();
    }
}
