// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.PropertiesApi.Application.Properties.Interfaces;

public interface IStorageService
{
    Task UploadAsync(string path, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
    Task<string> GenerateSignedUrlAsync(string path, TimeSpan expiry, CancellationToken cancellationToken = default);
}
