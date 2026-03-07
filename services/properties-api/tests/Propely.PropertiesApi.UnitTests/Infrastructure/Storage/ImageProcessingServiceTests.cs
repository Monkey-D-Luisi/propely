// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.PropertiesApi.Infrastructure.Storage;

namespace Propely.PropertiesApi.UnitTests.Infrastructure.Storage;

public class ImageProcessingServiceTests
{
    private readonly ImageProcessingService _service = new();

    [Fact]
    public async Task ResizeAsync_ReturnsCopiedStreamWithDimensions()
    {
        var inputData = new byte[] { 1, 2, 3, 4, 5 };
        using var input = new MemoryStream(inputData);

        var (resized, width, height) = await _service.ResizeAsync(input, 800);

        width.Should().Be(800);
        height.Should().Be(600);
        resized.Position.Should().Be(0);

        using var reader = new MemoryStream();
        await resized.CopyToAsync(reader);
        reader.ToArray().Should().BeEquivalentTo(inputData);
        await resized.DisposeAsync();
    }

    [Fact]
    public async Task ResizeAsync_SetsOutputPositionToZero()
    {
        using var input = new MemoryStream([10, 20, 30]);

        var (resized, _, _) = await _service.ResizeAsync(input, 2048);

        resized.Position.Should().Be(0);
        await resized.DisposeAsync();
    }

    [Fact]
    public async Task ResizeAsync_CalculatesHeightAs75PercentOfWidth()
    {
        using var input = new MemoryStream([1]);

        var (resized, width, height) = await _service.ResizeAsync(input, 400);

        width.Should().Be(400);
        height.Should().Be(300);
        await resized.DisposeAsync();
    }
}
