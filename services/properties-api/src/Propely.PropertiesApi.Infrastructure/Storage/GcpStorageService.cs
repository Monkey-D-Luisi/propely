// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Application.Properties.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Propely.PropertiesApi.Infrastructure.Storage;

public sealed class GcpStorageService : IStorageService
{
    private readonly ILogger<GcpStorageService> _logger;
    private readonly string _bucketName;

    public GcpStorageService(ILogger<GcpStorageService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _bucketName = configuration["Storage:BucketName"] ?? "propely-media-dev";
    }

    public async Task UploadAsync(string path, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading {Path} to bucket {Bucket}", path, _bucketName);
        // TODO: Implement real GCP Cloud Storage upload when infrastructure is ready
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting {Path} from bucket {Bucket}", path, _bucketName);
        // TODO: Implement real GCP Cloud Storage delete when infrastructure is ready
        await Task.CompletedTask;
    }

    public async Task<string> GenerateSignedUrlAsync(string path, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        // TODO: Implement real GCP signed URL generation
        var url = $"https://storage.googleapis.com/{_bucketName}/{path}?expiry={expiry.TotalSeconds}";
        return await Task.FromResult(url);
    }
}
