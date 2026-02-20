// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.FeatureFlags.Interfaces;
using Propely.OrgsApi.Application.FeatureFlags.Queries.CheckFeatureFlag;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.FeatureFlags;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.FeatureFlags.Queries;

public sealed class CheckFeatureFlagQueryHandlerTests
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagDefaults _defaults;
    private readonly CheckFeatureFlagQueryHandler _handler;

    public CheckFeatureFlagQueryHandlerTests()
    {
        _repository = Substitute.For<IFeatureFlagRepository>();
        _defaults = Substitute.For<IFeatureFlagDefaults>();
        _handler = new CheckFeatureFlagQueryHandler(_repository, _defaults);
    }

    [Fact]
    public async Task Handle_ShouldReturnConfigValue_WhenNoDbOverride()
    {
        // Arrange
        var configDefaults = new Dictionary<string, bool> { { "DarkMode", false } };
        _defaults.GetDefaults().Returns(configDefaults);
        _repository.GetByNameAsync("DarkMode", Arg.Any<CancellationToken>()).Returns((FeatureFlag?)null);

        // Act
        var result = await _handler.Handle(new CheckFeatureFlagQuery("DarkMode"), CancellationToken.None);

        // Assert
        result.Name.Should().Be("DarkMode");
        result.IsEnabled.Should().BeFalse();
        result.Source.Should().Be("Configuration");
    }

    [Fact]
    public async Task Handle_ShouldReturnDbValue_WhenDbOverrideExists()
    {
        // Arrange
        var configDefaults = new Dictionary<string, bool> { { "DarkMode", false } };
        _defaults.GetDefaults().Returns(configDefaults);
        var dbFlag = FeatureFlag.Create("DarkMode", true);
        _repository.GetByNameAsync("DarkMode", Arg.Any<CancellationToken>()).Returns(dbFlag);

        // Act
        var result = await _handler.Handle(new CheckFeatureFlagQuery("DarkMode"), CancellationToken.None);

        // Assert
        result.Name.Should().Be("DarkMode");
        result.IsEnabled.Should().BeTrue();
        result.Source.Should().Be("Database");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenFlagNotDefined()
    {
        // Arrange
        _defaults.GetDefaults().Returns(new Dictionary<string, bool>());

        // Act
        var act = () => _handler.Handle(new CheckFeatureFlagQuery("NonExistent"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
