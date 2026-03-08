// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AiApi.Domain.Suggestions;
using Propely.AiApi.Application.Suggestions.Rules;
using Propely.ContactsApi.Client;

namespace Propely.AiApi.UnitTests.Application.Suggestions.Rules;

public class LowConversionRuleTests
{
    private readonly ILeadsApiClient _leadsClient = Substitute.For<ILeadsApiClient>();
    private readonly LowConversionRule _rule;
    private static readonly Guid TenantId = Guid.NewGuid();

    public LowConversionRuleTests()
    {
        _rule = new LowConversionRule(_leadsClient);
    }

    [Fact]
    public async Task EvaluateAsync_WhenConversionBelowThreshold_ReturnsSuggestion()
    {
        _leadsClient.CountByStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int>
            {
                ["New"] = 5, ["Contacted"] = 3, ["Qualified"] = 2, ["Converted"] = 1, ["Lost"] = 4
            });

        var result = await _rule.EvaluateAsync(TenantId);

        result.Should().HaveCount(1);
        result[0].Type.Should().Be(SuggestionType.LowConversion);
        result[0].Priority.Should().Be(SuggestionPriority.High);
        result[0].Message.Should().Contain("6%");
    }

    [Fact]
    public async Task EvaluateAsync_WhenConversionAboveThreshold_ReturnsEmpty()
    {
        _leadsClient.CountByStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int>
            {
                ["New"] = 1, ["Contacted"] = 1, ["Qualified"] = 1, ["Converted"] = 5, ["Lost"] = 2
            });

        var result = await _rule.EvaluateAsync(TenantId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_WhenTooFewLeads_ReturnsEmpty()
    {
        _leadsClient.CountByStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int> { ["New"] = 2, ["Converted"] = 0 });

        var result = await _rule.EvaluateAsync(TenantId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_WhenExactlyAtThreshold_ReturnsEmpty()
    {
        // 20% exactly should NOT trigger (rate >= threshold)
        _leadsClient.CountByStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int>
            {
                ["New"] = 2, ["Contacted"] = 1, ["Qualified"] = 1, ["Converted"] = 2, ["Lost"] = 4
            });

        var result = await _rule.EvaluateAsync(TenantId);

        result.Should().BeEmpty();
    }
}
