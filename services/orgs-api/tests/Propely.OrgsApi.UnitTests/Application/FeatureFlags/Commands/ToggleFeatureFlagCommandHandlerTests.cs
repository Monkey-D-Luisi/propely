// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.FeatureFlags.Commands.ToggleFeatureFlag;
using Propely.OrgsApi.Application.FeatureFlags.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.FeatureFlags;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.FeatureFlags.Commands;

public sealed class ToggleFeatureFlagCommandHandlerTests
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFeatureFlagDefaults _defaults;
    private readonly ToggleFeatureFlagCommandHandler _handler;

    public ToggleFeatureFlagCommandHandlerTests()
    {
        _repository = Substitute.For<IFeatureFlagRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _defaults = Substitute.For<IFeatureFlagDefaults>();
        _handler = new ToggleFeatureFlagCommandHandler(_repository, _unitOfWork, _defaults);
    }

    [Fact]
    public async Task Handle_ShouldCreateNewDbOverride_WhenNoneExists()
    {
        // Arrange
        _defaults.IsDefinedFlag("DarkMode").Returns(true);
        _repository.GetByNameAsync("DarkMode", Arg.Any<CancellationToken>()).Returns((FeatureFlag?)null);

        var command = new ToggleFeatureFlagCommand("DarkMode", true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(
            Arg.Is<FeatureFlag>(f => f.Name == "DarkMode" && f.IsEnabled),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingDbOverride()
    {
        // Arrange
        _defaults.IsDefinedFlag("DarkMode").Returns(true);
        var existing = FeatureFlag.Create("DarkMode", false);
        _repository.GetByNameAsync("DarkMode", Arg.Any<CancellationToken>()).Returns(existing);

        var command = new ToggleFeatureFlagCommand("DarkMode", true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existing.IsEnabled.Should().BeTrue();
        await _repository.DidNotReceive().AddAsync(Arg.Any<FeatureFlag>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenFlagNotDefined()
    {
        // Arrange
        _defaults.IsDefinedFlag("NonExistent").Returns(false);

        var command = new ToggleFeatureFlagCommand("NonExistent", true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
