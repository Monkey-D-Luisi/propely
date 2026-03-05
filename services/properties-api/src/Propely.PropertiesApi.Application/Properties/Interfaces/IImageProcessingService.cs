// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.PropertiesApi.Application.Properties.Interfaces;

public interface IImageProcessingService
{
    Task<(Stream resized, int width, int height)> ResizeAsync(Stream input, int maxWidth, CancellationToken cancellationToken = default);
}
