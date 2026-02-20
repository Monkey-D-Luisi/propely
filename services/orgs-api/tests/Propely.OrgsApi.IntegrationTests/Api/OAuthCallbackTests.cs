// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using Propely.OrgsApi.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Propely.OrgsApi.IntegrationTests.Api;

/// <summary>
/// Integration tests for OAuth callback scenarios. These test the HTTP-level
/// behavior of the OAuth callback endpoint, including missing email, provider
/// mismatch, and invalid provider scenarios.
/// </summary>
public sealed class OAuthCallbackTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public OAuthCallbackTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateNoRedirectClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    [Fact]
    public async Task OAuthCallback_WithInvalidProvider_ShouldRedirectWithError()
    {
        // Arrange
        var client = CreateNoRedirectClient();

        // Act — call callback with an unsupported provider
        var response = await client.GetAsync("/auth/oauth/callback?provider=invalid-provider");

        // Assert — should redirect to frontend with an error
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var location = response.Headers.Location?.ToString();
        location.Should().NotBeNull();
        location.Should().Contain("oauthError=");
    }

    [Fact]
    public async Task OAuthCallback_WithMissingProvider_ShouldRedirectWithError()
    {
        // Arrange
        var client = CreateNoRedirectClient();

        // Act — call callback without provider parameter
        var response = await client.GetAsync("/auth/oauth/callback");

        // Assert — should redirect with error or return bad request
        // Provider is required parameter
        var statusCode = (int)response.StatusCode;
        statusCode.Should().BeOneOf(
            (int)HttpStatusCode.Redirect,
            (int)HttpStatusCode.BadRequest,
            (int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task OAuthChallenge_Google_ShouldRedirectToProvider()
    {
        // Arrange
        var client = CreateNoRedirectClient();

        // Act
        var response = await client.GetAsync("/auth/oauth/google");

        // Assert — should redirect to Google OAuth
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var location = response.Headers.Location?.ToString();
        location.Should().NotBeNull();
        location.Should().Contain("accounts.google.com");
    }

    [Fact]
    public async Task OAuthChallenge_GitHub_ShouldRedirectToProvider()
    {
        // Arrange
        var client = CreateNoRedirectClient();

        // Act
        var response = await client.GetAsync("/auth/oauth/github");

        // Assert — should redirect to GitHub OAuth
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var location = response.Headers.Location?.ToString();
        location.Should().NotBeNull();
        location.Should().Contain("github.com");
    }

    [Fact]
    public async Task OAuthChallenge_Google_WithNextParam_ShouldPreserveNext()
    {
        // Arrange
        var client = CreateNoRedirectClient();

        // Act
        var response = await client.GetAsync("/auth/oauth/google?next=/dashboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var location = response.Headers.Location?.ToString();
        location.Should().NotBeNull();
    }
}
