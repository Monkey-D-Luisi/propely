// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Propely.AiApi.Application.Actions.Tools;
using Propely.AiApi.Domain.Actions;
using Propely.AiApi.Infrastructure.AI;
using Propely.AiApi.Infrastructure.AI.Adapters;
using Propely.AiApi.Infrastructure.Services;

namespace Propely.AiApi.UnitTests.Application.Actions;

public sealed class OpenAiIntentClassifierTests
{
    private readonly IToolSchemaRegistry _registry = new ToolSchemaRegistry();
    private readonly OpenAiToolAdapter _adapter = new();

    [Fact]
    public async Task ClassifyAsync_WhenApiKeyNotConfigured_ShouldReturnUnknownIntent()
    {
        // Arrange
        var options = Options.Create(new OpenAiOptions { ApiKey = null, ModelId = "gpt-5-mini" });
        var logger = Substitute.For<ILogger<OpenAiIntentClassifier>>();
        var classifier = new OpenAiIntentClassifier(options, _registry, _adapter, logger);

        // Act
        var result = await classifier.ClassifyAsync("Create a 3 bedroom apartment in Malaga");

        // Assert
        result.ActionType.Should().Be(ActionType.Unknown);
        result.Confidence.Should().Be(0.0);
        result.Parameters.Should().BeEmpty();
    }

    [Fact]
    public async Task ClassifyAsync_WhenApiKeyIsEmpty_ShouldReturnUnknownIntent()
    {
        // Arrange
        var options = Options.Create(new OpenAiOptions { ApiKey = "", ModelId = "gpt-5-mini" });
        var logger = Substitute.For<ILogger<OpenAiIntentClassifier>>();
        var classifier = new OpenAiIntentClassifier(options, _registry, _adapter, logger);

        // Act
        var result = await classifier.ClassifyAsync("Show me all villas under 500k");

        // Assert
        result.ActionType.Should().Be(ActionType.Unknown);
        result.Confidence.Should().Be(0.0);
    }

    [Fact]
    public async Task ClassifyAsync_WhenApiKeyIsWhitespace_ShouldReturnUnknownIntent()
    {
        // Arrange
        var options = Options.Create(new OpenAiOptions { ApiKey = "   ", ModelId = "gpt-5-mini" });
        var logger = Substitute.For<ILogger<OpenAiIntentClassifier>>();
        var classifier = new OpenAiIntentClassifier(options, _registry, _adapter, logger);

        // Act
        var result = await classifier.ClassifyAsync("What properties do I have?");

        // Assert
        result.ActionType.Should().Be(ActionType.Unknown);
        result.Confidence.Should().Be(0.0);
        result.Parameters.Should().BeEmpty();
    }
}
