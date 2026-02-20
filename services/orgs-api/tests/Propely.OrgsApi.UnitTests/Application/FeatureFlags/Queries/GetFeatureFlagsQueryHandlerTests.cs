// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.FeatureFlags.Interfaces;
using Propely.OrgsApi.Application.FeatureFlags.Queries.GetFeatureFlags;
using Propely.OrgsApi.Domain.FeatureFlags;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.FeatureFlags.Queries;

public sealed class GetFeatureFlagsQueryHandlerTests
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagDefaults _defaults;
    private readonly GetFeatureFlagsQueryHandler _handler;

    public GetFeatureFlagsQueryHandlerTests()
    {
        _repository = Substitute.For<IFeatureFlagRepository>();
        _defaults = Substitute.For<IFeatureFlagDefaults>();
        _handler = new GetFeatureFlagsQueryHandler(_repository, _defaults);
    }

    [Fact]
    public async Task Handle_ShouldReturnConfigDefaultsWhenNoDbOverrides()
    {
        // Arrange
        var configDefaults = new Dictionary<string, bool>
        {
            { "Notifications", true },
            { "DarkMode", false }
        };
        _defaults.GetDefaults().Returns(configDefaults);
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<FeatureFlag>());

        // Act
        var result = await _handler.Handle(new GetFeatureFlagsQuery(), CancellationToken.None);

        // Assert
        result.Flags.Should().HaveCount(2);
        result.Flags.Should().Contain(f => f.Name == "DarkMode" && !f.IsEnabled && f.Source == "Configuration");
        result.Flags.Should().Contain(f => f.Name == "Notifications" && f.IsEnabled && f.Source == "Configuration");
    }

    [Fact]
    public async Task Handle_ShouldReturnDbOverrideWhenPresent()
    {
        // Arrange
        var configDefaults = new Dictionary<string, bool>
        {
            { "DarkMode", false }
        };
        _defaults.GetDefaults().Returns(configDefaults);
        var dbFlag = FeatureFlag.Create("DarkMode", true);
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<FeatureFlag> { dbFlag });

        // Act
        var result = await _handler.Handle(new GetFeatureFlagsQuery(), CancellationToken.None);

        // Assert
        result.Flags.Should().HaveCount(1);
        var flag = result.Flags[0];
        flag.Name.Should().Be("DarkMode");
        flag.IsEnabled.Should().BeTrue();
        flag.Source.Should().Be("Database");
    }

    [Fact]
    public async Task Handle_ShouldReturnFlagsSortedByName()
    {
        // Arrange
        var configDefaults = new Dictionary<string, bool>
        {
            { "Zebra", true },
            { "Alpha", false },
            { "Middle", true }
        };
        _defaults.GetDefaults().Returns(configDefaults);
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<FeatureFlag>());

        // Act
        var result = await _handler.Handle(new GetFeatureFlagsQuery(), CancellationToken.None);

        // Assert
        result.Flags.Select(f => f.Name).Should().BeInAscendingOrder();
    }
}
