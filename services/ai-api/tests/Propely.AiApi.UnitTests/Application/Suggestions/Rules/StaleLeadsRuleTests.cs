// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Propely.AiApi.Domain.Suggestions;
using Propely.AiApi.Application.Suggestions.Rules;
using Propely.ContactsApi.Client;

namespace Propely.AiApi.UnitTests.Application.Suggestions.Rules;

public class StaleLeadsRuleTests
{
    private readonly ILeadsApiClient _leadsClient = Substitute.For<ILeadsApiClient>();
    private readonly StaleLeadsRule _rule;
    private static readonly Guid TenantId = Guid.NewGuid();

    public StaleLeadsRuleTests()
    {
        _rule = new StaleLeadsRule(_leadsClient);
    }

    [Fact]
    public async Task EvaluateAsync_WhenNewLeadsAboveThreshold_ReturnsSuggestion()
    {
        _leadsClient.CountByStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int> { ["New"] = 8, ["Contacted"] = 2 });

        var result = await _rule.EvaluateAsync(TenantId);

        result.Should().HaveCount(1);
        result[0].Type.Should().Be(SuggestionType.StaleLeads);
        result[0].Priority.Should().Be(SuggestionPriority.High);
        result[0].Message.Should().Contain("8 new leads");
    }

    [Fact]
    public async Task EvaluateAsync_WhenNewLeadsBelowThreshold_ReturnsEmpty()
    {
        _leadsClient.CountByStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int> { ["New"] = 2 });

        var result = await _rule.EvaluateAsync(TenantId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_WhenNoNewLeads_ReturnsEmpty()
    {
        _leadsClient.CountByStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, int> { ["Contacted"] = 5 });

        var result = await _rule.EvaluateAsync(TenantId);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Type_ShouldBeStaleLeads()
    {
        _rule.Type.Should().Be(SuggestionType.StaleLeads);
    }
}
