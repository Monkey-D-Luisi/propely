// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Application.Properties.Interfaces;

namespace Propely.PropertiesApi.Infrastructure.Storage;

public sealed class ImageProcessingService : IImageProcessingService
{
    public async Task<(Stream resized, int width, int height)> ResizeAsync(Stream input, int maxWidth, CancellationToken cancellationToken = default)
    {
        // TODO: Implement real image resizing with SkiaSharp or ImageSharp when GCP infra is ready
        // For now, return the input stream as-is
        var output = new MemoryStream();
        await input.CopyToAsync(output, cancellationToken);
        output.Position = 0;
        return (output, maxWidth, (int)(maxWidth * 0.75));
    }
}
