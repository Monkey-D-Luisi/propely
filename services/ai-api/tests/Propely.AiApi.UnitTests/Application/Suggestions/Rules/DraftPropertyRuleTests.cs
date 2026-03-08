// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AiApi.Domain.Suggestions;
using Propely.AiApi.Application.Suggestions.Rules;
using Propely.PropertiesApi.Client;

namespace Propely.AiApi.UnitTests.Application.Suggestions.Rules;

public class DraftPropertyRuleTests
{
    private readonly IPropertiesApiClient _propertiesClient = Substitute.For<IPropertiesApiClient>();
    private readonly DraftPropertyRule _rule;
    private static readonly Guid TenantId = Guid.NewGuid();

    public DraftPropertyRuleTests()
    {
        _rule = new DraftPropertyRule(_propertiesClient);
    }

    [Fact]
    public async Task EvaluateAsync_WhenDraftAboveThreshold_ReturnsSuggestion()
    {
        _propertiesClient.CountByStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int> { ["Draft"] = 5, ["Active"] = 3 });

        var result = await _rule.EvaluateAsync(TenantId);

        result.Should().HaveCount(1);
        result[0].Type.Should().Be(SuggestionType.DraftProperty);
        result[0].Priority.Should().Be(SuggestionPriority.Medium);
        result[0].Message.Should().Contain("5 properties in Draft");
    }

    [Fact]
    public async Task EvaluateAsync_WhenDraftBelowThreshold_ReturnsEmpty()
    {
        _propertiesClient.CountByStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int> { ["Draft"] = 1, ["Active"] = 10 });

        var result = await _rule.EvaluateAsync(TenantId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_WhenNoDraftProperties_ReturnsEmpty()
    {
        _propertiesClient.CountByStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int> { ["Active"] = 10 });

        var result = await _rule.EvaluateAsync(TenantId);

        result.Should().BeEmpty();
    }
}
