// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.AiApi.Domain.Suggestions;
using Propely.AiApi.Application.Suggestions.Interfaces;
using Propely.AiApi.Application.Suggestions.Rules;

namespace Propely.AiApi.UnitTests.Application.Suggestions.Rules;

public class SuggestionEngineTests
{
    private readonly ILogger<SuggestionEngine> _logger = Substitute.For<ILogger<SuggestionEngine>>();
    private static readonly Guid TenantId = Guid.NewGuid();

    [Fact]
    public async Task GenerateAsync_AggregatesAllRuleResults()
    {
        var rule1 = Substitute.For<ISuggestionRule>();
        rule1.Type.Returns(SuggestionType.StaleLeads);
        rule1.EvaluateAsync(TenantId, Arg.Any<CancellationToken>())
            .Returns([new Suggestion(SuggestionType.StaleLeads, SuggestionPriority.High, "Stale leads")]);

        var rule2 = Substitute.For<ISuggestionRule>();
        rule2.Type.Returns(SuggestionType.EmptyCalendar);
        rule2.EvaluateAsync(TenantId, Arg.Any<CancellationToken>())
            .Returns([new Suggestion(SuggestionType.EmptyCalendar, SuggestionPriority.Medium, "Empty calendar")]);

        var engine = new SuggestionEngine([rule1, rule2], _logger);

        var result = await engine.GenerateAsync(TenantId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GenerateAsync_OrdersByPriorityDescending()
    {
        var lowRule = Substitute.For<ISuggestionRule>();
        lowRule.Type.Returns(SuggestionType.DraftProperty);
        lowRule.EvaluateAsync(TenantId, Arg.Any<CancellationToken>())
            .Returns([new Suggestion(SuggestionType.DraftProperty, SuggestionPriority.Low, "Low prio")]);

        var highRule = Substitute.For<ISuggestionRule>();
        highRule.Type.Returns(SuggestionType.StaleLeads);
        highRule.EvaluateAsync(TenantId, Arg.Any<CancellationToken>())
            .Returns([new Suggestion(SuggestionType.StaleLeads, SuggestionPriority.High, "High prio")]);

        var engine = new SuggestionEngine([lowRule, highRule], _logger);

        var result = await engine.GenerateAsync(TenantId);

        result[0].Priority.Should().Be(SuggestionPriority.High);
        result[1].Priority.Should().Be(SuggestionPriority.Low);
    }

    [Fact]
    public async Task GenerateAsync_ContinuesWhenRuleFails()
    {
        var failingRule = Substitute.For<ISuggestionRule>();
        failingRule.Type.Returns(SuggestionType.StaleLeads);
        failingRule.EvaluateAsync(TenantId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var successRule = Substitute.For<ISuggestionRule>();
        successRule.Type.Returns(SuggestionType.EmptyCalendar);
        successRule.EvaluateAsync(TenantId, Arg.Any<CancellationToken>())
            .Returns([new Suggestion(SuggestionType.EmptyCalendar, SuggestionPriority.Medium, "No appointments")]);

        var engine = new SuggestionEngine([failingRule, successRule], _logger);

        var result = await engine.GenerateAsync(TenantId);

        result.Should().HaveCount(1);
        result[0].Type.Should().Be(SuggestionType.EmptyCalendar);
    }

    [Fact]
    public async Task GenerateAsync_ReturnsEmptyWhenNoRulesMatch()
    {
        var rule = Substitute.For<ISuggestionRule>();
        rule.Type.Returns(SuggestionType.StaleLeads);
        rule.EvaluateAsync(TenantId, Arg.Any<CancellationToken>())
            .Returns(new List<Suggestion>());

        var engine = new SuggestionEngine([rule], _logger);

        var result = await engine.GenerateAsync(TenantId);

        result.Should().BeEmpty();
    }
}
