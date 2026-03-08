// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.AiApi.Domain.Suggestions;
using Propely.AiApi.Application.Suggestions.Rules;
using Propely.ContactsApi.Client;
using Propely.ContactsApi.Client.Models;

namespace Propely.AiApi.UnitTests.Application.Suggestions.Rules;

public class GroupedViewingRuleTests
{
    private readonly IContactsApiClient _contactsClient = Substitute.For<IContactsApiClient>();
    private readonly GroupedViewingRule _rule;
    private static readonly Guid TenantId = Guid.NewGuid();

    public GroupedViewingRuleTests()
    {
        _rule = new GroupedViewingRule(_contactsClient);
    }

    [Fact]
    public async Task EvaluateAsync_WhenContactHasEnoughInterests_ReturnsSuggestion()
    {
        var interests = new List<PropertyInterestResponse>
        {
            new() { Id = Guid.NewGuid(), PropertyId = Guid.NewGuid(), InterestType = "Buying" },
            new() { Id = Guid.NewGuid(), PropertyId = Guid.NewGuid(), InterestType = "Buying" },
            new() { Id = Guid.NewGuid(), PropertyId = Guid.NewGuid(), InterestType = "Renting" },
        };

        var contacts = new ContactListResponse
        {
            Items = [new ContactResponse
            {
                Id = Guid.NewGuid(), FirstName = "Maria", LastName = "Garcia",
                PropertyInterests = interests
            }]
        };

        _contactsClient.GetContactsAsync(pageSize: 100, ct: Arg.Any<CancellationToken>())
            .Returns(contacts);

        var result = await _rule.EvaluateAsync(TenantId);

        result.Should().HaveCount(1);
        result[0].Type.Should().Be(SuggestionType.GroupedViewing);
        result[0].Message.Should().Contain("Maria Garcia");
        result[0].Message.Should().Contain("3 properties");
    }

    [Fact]
    public async Task EvaluateAsync_WhenContactHasFewInterests_ReturnsEmpty()
    {
        var contacts = new ContactListResponse
        {
            Items = [new ContactResponse
            {
                Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe",
                PropertyInterests = [new PropertyInterestResponse { Id = Guid.NewGuid(), PropertyId = Guid.NewGuid(), InterestType = "Buying" }]
            }]
        };

        _contactsClient.GetContactsAsync(pageSize: 100, ct: Arg.Any<CancellationToken>())
            .Returns(contacts);

        var result = await _rule.EvaluateAsync(TenantId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_WhenNoContacts_ReturnsEmpty()
    {
        _contactsClient.GetContactsAsync(pageSize: 100, ct: Arg.Any<CancellationToken>())
            .Returns(new ContactListResponse { Items = [] });

        var result = await _rule.EvaluateAsync(TenantId);

        result.Should().BeEmpty();
    }
}
