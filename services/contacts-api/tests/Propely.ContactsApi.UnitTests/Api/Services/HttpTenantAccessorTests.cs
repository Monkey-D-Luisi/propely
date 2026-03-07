// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Propely.ContactsApi.Api.Services;

namespace Propely.ContactsApi.UnitTests.Api.Services;

public sealed class HttpTenantAccessorTests
{
    [Fact]
    public void GetCurrentOrgId_WhenNoHttpContext_ShouldReturnNull()
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        var accessor = new HttpTenantAccessor(httpContextAccessor);

        var result = accessor.GetCurrentOrgId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentOrgId_WhenValidOrgIdClaim_ShouldReturnGuid()
    {
        var orgId = Guid.NewGuid();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("org_id", orgId.ToString())
            }))
        };
        httpContextAccessor.HttpContext.Returns(context);
        var accessor = new HttpTenantAccessor(httpContextAccessor);

        var result = accessor.GetCurrentOrgId();

        result.Should().Be(orgId);
    }

    [Fact]
    public void GetCurrentOrgId_WhenNoOrgIdClaim_ShouldReturnNull()
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", "user123")
            }))
        };
        httpContextAccessor.HttpContext.Returns(context);
        var accessor = new HttpTenantAccessor(httpContextAccessor);

        var result = accessor.GetCurrentOrgId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentOrgId_WhenInvalidOrgIdClaim_ShouldReturnNull()
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("org_id", "not-a-guid")
            }))
        };
        httpContextAccessor.HttpContext.Returns(context);
        var accessor = new HttpTenantAccessor(httpContextAccessor);

        var result = accessor.GetCurrentOrgId();

        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentOrgId_WhenEmptyGuidClaim_ShouldReturnNull()
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("org_id", Guid.Empty.ToString())
            }))
        };
        httpContextAccessor.HttpContext.Returns(context);
        var accessor = new HttpTenantAccessor(httpContextAccessor);

        var result = accessor.GetCurrentOrgId();

        result.Should().BeNull();
    }
}
