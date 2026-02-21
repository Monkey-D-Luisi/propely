// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Propely.PublishingApi.Api.Middleware;

namespace Propely.PublishingApi.UnitTests.Api.Middleware;

public sealed class OrgContextMiddlewareTests
{
    private OrgContextMiddleware CreateMiddleware(RequestDelegate? next = null)
    {
        return new OrgContextMiddleware(next ?? (_ => Task.CompletedTask));
    }

    private static DefaultHttpContext CreateAuthenticatedContext(
        string? orgIdHeader = null,
        string? existingOrgId = null,
        string? orgIds = null)
    {
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new("sub", Guid.NewGuid().ToString())
        };

        if (existingOrgId is not null)
            claims.Add(new Claim("org_id", existingOrgId));

        if (orgIds is not null)
            claims.Add(new Claim("org_ids", orgIds));

        var identity = new ClaimsIdentity(claims, "TestScheme");
        context.User = new ClaimsPrincipal(identity);

        if (orgIdHeader is not null)
            context.Request.Headers["X-Org-Id"] = orgIdHeader;

        return context;
    }

    [Fact]
    public async Task InvokeAsync_WhenNotAuthenticated_ShouldCallNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenOrgIdAlreadyPresent_ShouldNotModifyClaims()
    {
        var orgId = Guid.NewGuid().ToString();
        var context = CreateAuthenticatedContext(existingOrgId: orgId);
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        var orgIdClaims = context.User.FindAll("org_id").ToList();
        orgIdClaims.Should().HaveCount(1);
        orgIdClaims[0].Value.Should().Be(orgId);
    }

    [Fact]
    public async Task InvokeAsync_WhenValidHeader_AndMemberOfOrg_ShouldAddClaim()
    {
        var orgId = Guid.NewGuid();
        var context = CreateAuthenticatedContext(
            orgIdHeader: orgId.ToString(),
            orgIds: orgId.ToString());
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        context.User.FindFirst("org_id")!.Value.Should().Be(orgId.ToString());
    }

    [Fact]
    public async Task InvokeAsync_WhenValidHeader_AndNotMemberOfOrg_ShouldNotAddClaim()
    {
        var orgId = Guid.NewGuid();
        var otherOrgId = Guid.NewGuid();
        var context = CreateAuthenticatedContext(
            orgIdHeader: orgId.ToString(),
            orgIds: otherOrgId.ToString());
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        context.User.FindFirst("org_id").Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WhenValidHeader_AndNoOrgIdsClaim_ShouldAddClaim()
    {
        var orgId = Guid.NewGuid();
        var context = CreateAuthenticatedContext(orgIdHeader: orgId.ToString());
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        context.User.FindFirst("org_id")!.Value.Should().Be(orgId.ToString());
    }

    [Fact]
    public async Task InvokeAsync_WhenInvalidHeader_ShouldNotAddClaim()
    {
        var context = CreateAuthenticatedContext(orgIdHeader: "not-a-guid");
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        context.User.FindFirst("org_id").Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WhenNoHeader_ShouldNotAddClaim()
    {
        var context = CreateAuthenticatedContext();
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        context.User.FindFirst("org_id").Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WhenMemberOfMultipleOrgs_ShouldAddMatchingClaim()
    {
        var orgId1 = Guid.NewGuid();
        var orgId2 = Guid.NewGuid();
        var context = CreateAuthenticatedContext(
            orgIdHeader: orgId2.ToString(),
            orgIds: $"{orgId1},{orgId2}");
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        context.User.FindFirst("org_id")!.Value.Should().Be(orgId2.ToString());
    }
}
