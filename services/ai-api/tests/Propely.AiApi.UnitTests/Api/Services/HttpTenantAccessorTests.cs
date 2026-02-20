// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using Propely.AiApi.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Propely.AiApi.UnitTests.Api.Services;

public sealed class HttpTenantAccessorTests
{
    [Fact]
    public void GetCurrentOrgId_WithValidOrgIdClaim_ShouldReturnOrgId()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var accessor = CreateAccessorWithClaims(new Claim("org_id", orgId.ToString()));

        // Act
        var result = accessor.GetCurrentOrgId();

        // Assert
        result.Should().Be(orgId);
    }

    [Fact]
    public void GetCurrentOrgId_WithNoHttpContext_ShouldReturnNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        var accessor = new HttpTenantAccessor(httpContextAccessor);

        // Act
        var result = accessor.GetCurrentOrgId();

        // Assert — null = system mode, bypasses tenant filter
        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentOrgId_WithNoOrgIdClaim_ShouldReturnGuidEmpty()
    {
        // Arrange — HTTP context exists but org_id claim is missing
        var accessor = CreateAccessorWithClaims(new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()));

        // Act
        var result = accessor.GetCurrentOrgId();

        // Assert — fail-closed: Guid.Empty matches no tenant
        result.Should().Be(Guid.Empty);
    }

    [Fact]
    public void GetCurrentOrgId_WithInvalidOrgIdClaim_ShouldReturnGuidEmpty()
    {
        // Arrange
        var accessor = CreateAccessorWithClaims(new Claim("org_id", "not-a-guid"));

        // Act
        var result = accessor.GetCurrentOrgId();

        // Assert — fail-closed: Guid.Empty matches no tenant
        result.Should().Be(Guid.Empty);
    }

    [Fact]
    public void GetCurrentOrgId_WithEmptyOrgIdClaim_ShouldReturnGuidEmpty()
    {
        // Arrange
        var accessor = CreateAccessorWithClaims(new Claim("org_id", ""));

        // Act
        var result = accessor.GetCurrentOrgId();

        // Assert — fail-closed: Guid.Empty matches no tenant
        result.Should().Be(Guid.Empty);
    }

    [Fact]
    public void GetCurrentOrgId_WithGuidEmptyOrgIdClaim_ShouldReturnGuidEmpty()
    {
        // Arrange — Guid.Empty is not a valid tenant ID
        var accessor = CreateAccessorWithClaims(new Claim("org_id", Guid.Empty.ToString()));

        // Act
        var result = accessor.GetCurrentOrgId();

        // Assert — fail-closed: Guid.Empty matches no tenant
        result.Should().Be(Guid.Empty);
    }

    private static HttpTenantAccessor CreateAccessorWithClaims(params Claim[] claims)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestScheme"))
        };

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);

        return new HttpTenantAccessor(httpContextAccessor);
    }
}
