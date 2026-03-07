// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.PropertiesApi.Api.Dtos;

namespace Propely.PropertiesApi.UnitTests.Api.Dtos;

public class PropertyMediaRequestsTests
{
    [Fact]
    public void ReorderRequest_CanBeInstantiated_WithItems()
    {
        var mediaId = Guid.NewGuid();
        var request = new ReorderRequest
        {
            Items =
            [
                new ReorderItemRequest { MediaId = mediaId, DisplayOrder = 0 },
                new ReorderItemRequest { MediaId = Guid.NewGuid(), DisplayOrder = 1 }
            ]
        };

        request.Items.Should().HaveCount(2);
        request.Items[0].MediaId.Should().Be(mediaId);
        request.Items[0].DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void ReorderRequest_DefaultItems_IsEmpty()
    {
        var request = new ReorderRequest();

        request.Items.Should().BeEmpty();
    }

    [Fact]
    public void ReorderItemRequest_CanSetProperties()
    {
        var id = Guid.NewGuid();
        var item = new ReorderItemRequest { MediaId = id, DisplayOrder = 5 };

        item.MediaId.Should().Be(id);
        item.DisplayOrder.Should().Be(5);
    }
}
