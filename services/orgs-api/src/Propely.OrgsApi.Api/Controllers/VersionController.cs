// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Api.Configuration;
using Propely.OrgsApi.Application.FeatureFlags.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Propely.OrgsApi.Api.Controllers;

[ApiController]
[Route("version")]
[Authorize]
public sealed class VersionController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlagService;
    private readonly VersionOptions _versionOptions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<VersionController> _logger;

    public VersionController(
        IFeatureFlagService featureFlagService,
        IOptions<VersionOptions> versionOptions,
        IHttpClientFactory httpClientFactory,
        ILogger<VersionController> logger)
    {
        _featureFlagService = featureFlagService;
        _versionOptions = versionOptions.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetVersion()
    {
        return Ok(new
        {
            version = _versionOptions.Current,
            buildDate = _versionOptions.BuildDate
        });
    }

    [HttpGet("check")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> CheckForUpdates(CancellationToken cancellationToken)
    {
        var updateCheckEnabled = await _featureFlagService.IsEnabledAsync("UpdateCheck");
        if (!updateCheckEnabled)
        {
            return Ok(new
            {
                enabled = false,
                message = "Update check is disabled. Enable the 'UpdateCheck' feature flag to check for updates."
            });
        }

        if (string.IsNullOrEmpty(_versionOptions.GitHubRepo))
        {
            return Ok(new
            {
                enabled = true,
                current = _versionOptions.Current,
                latest = (string?)null,
                updateAvailable = false,
                message = "GitHub repository not configured. Set Version:GitHubRepo to enable update checks."
            });
        }

        try
        {
            var client = _httpClientFactory.CreateClient("github");
            var response = await client.GetAsync(
                $"https://api.github.com/repos/{_versionOptions.GitHubRepo}/releases/latest",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Ok(new
                {
                    enabled = true,
                    current = _versionOptions.Current,
                    latest = (string?)null,
                    updateAvailable = false,
                    message = $"Could not reach GitHub API (HTTP {(int)response.StatusCode})."
                });
            }

            var release = await response.Content.ReadFromJsonAsync<GitHubRelease>(cancellationToken: cancellationToken);
            var latestVersion = release?.TagName?.TrimStart('v') ?? "";

            // System.Version handles dot-separated numeric versions (e.g. 1.2.3) but does
            // not support SemVer pre-release suffixes (e.g. 1.0.0-beta). Pre-release tags
            // will fail TryParse and be treated as "no update available".
            var isNewer = !string.IsNullOrEmpty(latestVersion)
                && Version.TryParse(latestVersion, out var latest)
                && Version.TryParse(_versionOptions.Current, out var current)
                && latest > current;

            return Ok(new
            {
                enabled = true,
                current = _versionOptions.Current,
                latest = latestVersion,
                updateAvailable = isNewer,
                releaseUrl = release?.HtmlUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check for updates from GitHub");
            return Ok(new
            {
                enabled = true,
                current = _versionOptions.Current,
                latest = (string?)null,
                updateAvailable = false,
                message = "Failed to check for updates. See server logs for details."
            });
        }
    }

    private sealed record GitHubRelease(
        [property: System.Text.Json.Serialization.JsonPropertyName("tag_name")] string? TagName,
        [property: System.Text.Json.Serialization.JsonPropertyName("html_url")] string? HtmlUrl);
}
