// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Application.Properties.Queries.CountPropertiesByStatus;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Properties.Queries;

public class CountPropertiesByStatusQueryHandlerTests
{
    private readonly IPropertyReadRepository _readRepository = Substitute.For<IPropertyReadRepository>();
    private readonly CountPropertiesByStatusQueryHandler _handler;

    public CountPropertiesByStatusQueryHandlerTests()
    {
        _handler = new CountPropertiesByStatusQueryHandler(_readRepository);
    }

    [Fact]
    public async Task Handle_ReturnsCounts_GroupedByStatusAsStrings()
    {
        var tenantId = Guid.NewGuid();
        var counts = new Dictionary<PropertyStatus, int>
        {
            { PropertyStatus.Draft, 5 },
            { PropertyStatus.Active, 10 },
            { PropertyStatus.Sold, 3 }
        };

        _readRepository.CountByStatusAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(counts);

        var result = await _handler.Handle(new CountPropertiesByStatusQuery(tenantId), CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().ContainKey("Draft").WhoseValue.Should().Be(5);
        result.Should().ContainKey("Active").WhoseValue.Should().Be(10);
        result.Should().ContainKey("Sold").WhoseValue.Should().Be(3);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyDictionary()
    {
        _readRepository.CountByStatusAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<PropertyStatus, int>());

        var result = await _handler.Handle(new CountPropertiesByStatusQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
