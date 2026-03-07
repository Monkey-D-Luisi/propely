// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Propely.PropertiesApi.Infrastructure.Storage;

namespace Propely.PropertiesApi.UnitTests.Infrastructure.Storage;

public class GcpStorageServiceTests
{
    private readonly ILogger<GcpStorageService> _logger = Substitute.For<ILogger<GcpStorageService>>();

    private GcpStorageService CreateService(string? bucketName = null)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(bucketName is not null
                ? new Dictionary<string, string?> { ["Storage:BucketName"] = bucketName }
                : [])
            .Build();
        return new GcpStorageService(_logger, config);
    }

    [Fact]
    public async Task UploadAsync_CompletesWithoutError()
    {
        var service = CreateService("test-bucket");
        using var stream = new MemoryStream([1, 2, 3]);

        var act = () => service.UploadAsync("path/file.jpg", stream, "image/jpeg");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_CompletesWithoutError()
    {
        var service = CreateService("test-bucket");

        var act = () => service.DeleteAsync("path/file.jpg");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GenerateSignedUrlAsync_ReturnsUrlWithBucketAndPath()
    {
        var service = CreateService("my-bucket");

        var url = await service.GenerateSignedUrlAsync("path/file.jpg", TimeSpan.FromHours(1));

        url.Should().Contain("my-bucket");
        url.Should().Contain("path/file.jpg");
    }

    [Fact]
    public async Task GenerateSignedUrlAsync_UsesDefaultBucket_WhenNotConfigured()
    {
        var service = CreateService();

        var url = await service.GenerateSignedUrlAsync("path/file.jpg", TimeSpan.FromMinutes(30));

        url.Should().Contain("propely-media-dev");
    }
}
