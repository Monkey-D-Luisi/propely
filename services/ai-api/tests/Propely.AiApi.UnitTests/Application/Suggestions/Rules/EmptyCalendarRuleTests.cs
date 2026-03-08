// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AiApi.Domain.Suggestions;
using Propely.AiApi.Application.Suggestions.Rules;
using Propely.AppointmentsApi.Client;
using Propely.AppointmentsApi.Client.Models;

namespace Propely.AiApi.UnitTests.Application.Suggestions.Rules;

public class EmptyCalendarRuleTests
{
    private readonly IAppointmentsApiClient _appointmentsClient = Substitute.For<IAppointmentsApiClient>();
    private readonly EmptyCalendarRule _rule;
    private static readonly Guid TenantId = Guid.NewGuid();

    public EmptyCalendarRuleTests()
    {
        _rule = new EmptyCalendarRule(_appointmentsClient);
    }

    [Fact]
    public async Task EvaluateAsync_WhenNoUpcoming_ReturnsSuggestion()
    {
        _appointmentsClient.CountUpcomingAsync(7, Arg.Any<CancellationToken>())
            .Returns(new CountUpcomingResponse(0));

        var result = await _rule.EvaluateAsync(TenantId);

        result.Should().HaveCount(1);
        result[0].Type.Should().Be(SuggestionType.EmptyCalendar);
        result[0].Priority.Should().Be(SuggestionPriority.Medium);
        result[0].Message.Should().Contain("no appointments");
    }

    [Fact]
    public async Task EvaluateAsync_WhenHasUpcoming_ReturnsEmpty()
    {
        _appointmentsClient.CountUpcomingAsync(7, Arg.Any<CancellationToken>())
            .Returns(new CountUpcomingResponse(3));

        var result = await _rule.EvaluateAsync(TenantId);

        result.Should().BeEmpty();
    }
}
