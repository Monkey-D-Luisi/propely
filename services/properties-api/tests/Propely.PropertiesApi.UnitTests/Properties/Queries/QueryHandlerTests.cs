// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.PropertiesApi.Application.Common.Models;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Application.Properties.Queries.GetPropertyById;
using Propely.PropertiesApi.Application.Properties.Queries.ListProperties;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Properties.Queries;

public class GetPropertyByIdQueryHandlerTests
{
    private readonly IPropertyReadRepository _readRepository = Substitute.For<IPropertyReadRepository>();
    private readonly GetPropertyByIdQueryHandler _handler;

    public GetPropertyByIdQueryHandlerTests()
    {
        _handler = new GetPropertyByIdQueryHandler(_readRepository);
    }

    [Fact]
    public async Task Handle_PropertyExists_ReturnsDto()
    {
        var tenantId = Guid.NewGuid();
        var property = Property.Create("Test", PropertyType.Apartment, OperationType.Sale, tenantId, Guid.NewGuid());

        _readRepository.GetByIdAsync(property.Id, tenantId, Arg.Any<CancellationToken>())
            .Returns(property);

        var result = await _handler.Handle(new GetPropertyByIdQuery(property.Id, tenantId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Test");
        result.Id.Should().Be(property.Id);
    }

    [Fact]
    public async Task Handle_PropertyNotFound_ReturnsNull()
    {
        _readRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Property?)null);

        var result = await _handler.Handle(new GetPropertyByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}

public class ListPropertiesQueryHandlerTests
{
    private readonly IPropertyReadRepository _readRepository = Substitute.For<IPropertyReadRepository>();
    private readonly ListPropertiesQueryHandler _handler;

    public ListPropertiesQueryHandlerTests()
    {
        _handler = new ListPropertiesQueryHandler(_readRepository);
    }

    [Fact]
    public async Task Handle_ReturnsPagedListItems()
    {
        var tenantId = Guid.NewGuid();
        var properties = new[]
        {
            Property.Create("A", PropertyType.Apartment, OperationType.Sale, tenantId, Guid.NewGuid()),
            Property.Create("B", PropertyType.Villa, OperationType.Rent, tenantId, Guid.NewGuid())
        };

        _readRepository.ListAsync(Arg.Any<PropertyListFilter>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Property>(properties, 2, 1, 20));

        var query = new ListPropertiesQuery { TenantId = tenantId, Page = 1, PageSize = 20 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.First().Title.Should().Be("A");
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _readRepository.ListAsync(Arg.Any<PropertyListFilter>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Property>([], 0, 1, 20));

        var query = new ListPropertiesQuery { TenantId = Guid.NewGuid() };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
}
