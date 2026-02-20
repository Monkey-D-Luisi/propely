// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Propely.OrgsApi.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Propely.OrgsApi.IntegrationTests.Api;

/// <summary>
/// Integration tests for BillingController endpoints.
/// IPaymentService is mocked in ApiWebApplicationFactory with IsEnabled = true.
/// </summary>
public sealed class BillingEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public BillingEndpointTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    private static string UniqueEmail() => $"billing-{Guid.NewGuid():N}@example.com";

    private static int _ipCounter;
    private static string UniqueIpAddress()
    {
        var suffix = System.Threading.Interlocked.Increment(ref _ipCounter);
        suffix = ((suffix - 1) % 253) + 1;
        return $"203.0.113.{suffix}";
    }

    private static async Task<string> GetCsrfTokenAsync(HttpClient client, string? ip = null)
    {
        ip ??= UniqueIpAddress();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/auth/csrf");
        request.Headers.Add("X-Real-IP", ip);
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("csrfToken").GetString()!;
    }

    private static async Task<(Guid UserId, string Email)> RegisterUserAsync(
        HttpClient client, string? email = null)
    {
        var ip = UniqueIpAddress();
        var csrf = await GetCsrfTokenAsync(client, ip);
        email ??= UniqueEmail();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register")
        {
            Content = JsonContent.Create(new { email, password = "TestPassword123!", name = "Test User" })
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", ip);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var userId = Guid.Parse(body.GetProperty("userId").GetString()!);
        return (userId, email);
    }

    private static async Task<HttpResponseMessage> SendPostWithCsrfAsync(
        HttpClient client, string path, object payload)
    {
        var ip = UniqueIpAddress();
        var csrf = await GetCsrfTokenAsync(client, ip);
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("x-csrf-token", csrf);
        request.Headers.Add("X-Real-IP", ip);
        return await client.SendAsync(request);
    }

    private static async Task<string> CreateOrgAsync(HttpClient client, string? name = null)
    {
        name ??= $"BillingOrg_{Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/orgs", new { name });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetString()!;
    }

    // ───────── GET /billing/plans ─────────

    [Fact]
    public async Task GetPlans_WhenNotAuthenticated_ShouldReturn200()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/billing/plans");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var plans = await response.Content.ReadFromJsonAsync<JsonElement>();
        plans.GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetPlans_ShouldReturnConfiguredPlans()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/billing/plans");
        response.EnsureSuccessStatusCode();

        var plans = await response.Content.ReadFromJsonAsync<JsonElement>();
        var planList = plans.EnumerateArray().ToList();

        planList.Should().Contain(p => p.GetProperty("id").GetString() == "free");
        planList.Should().Contain(p => p.GetProperty("id").GetString() == "pro");

        var proPlan = planList.First(p => p.GetProperty("id").GetString() == "pro");
        proPlan.GetProperty("name").GetString().Should().Be("Pro");
        proPlan.GetProperty("features").GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    // ───────── GET /billing/subscription ─────────

    [Fact]
    public async Task GetSubscription_WhenNotAuthenticated_ShouldReturn401()
    {
        var client = CreateClient();

        var response = await client.GetAsync($"/billing/subscription?orgId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSubscription_WithEmptyOrgId_ShouldReturn400()
    {
        var client = CreateClient();
        await RegisterUserAsync(client);

        var response = await client.GetAsync($"/billing/subscription?orgId={Guid.Empty}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSubscription_WhenNotMember_ShouldReturn403()
    {
        var clientA = CreateClient();
        await RegisterUserAsync(clientA);
        var orgId = await CreateOrgAsync(clientA);

        var clientB = CreateClient();
        await RegisterUserAsync(clientB);

        var response = await clientB.GetAsync($"/billing/subscription?orgId={orgId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSubscription_WhenMember_ShouldReturnFreeSubscription()
    {
        var client = CreateClient();
        await RegisterUserAsync(client);
        var orgId = await CreateOrgAsync(client);

        var response = await client.GetAsync($"/billing/subscription?orgId={orgId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("planId").GetString().Should().Be("free");
        body.GetProperty("status").GetString().Should().Be("free");
    }

    // ───────── POST /billing/checkout ─────────

    [Fact]
    public async Task CreateCheckout_WhenNotAuthenticated_ShouldReturn401()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/billing/checkout", new
        {
            orgId = Guid.NewGuid(),
            planId = "pro",
            successUrl = "/en/billing/success",
            cancelUrl = "/en/billing/cancel"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCheckout_WithoutCsrf_ShouldReturn403()
    {
        var client = CreateClient();
        await RegisterUserAsync(client);
        var orgId = await CreateOrgAsync(client);

        // POST without CSRF token
        var response = await client.PostAsJsonAsync("/billing/checkout", new
        {
            orgId,
            planId = "pro",
            successUrl = "/en/billing/success",
            cancelUrl = "/en/billing/cancel"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCheckout_WithAbsoluteUrl_ShouldReturnValidationError()
    {
        var client = CreateClient();
        await RegisterUserAsync(client);
        var orgId = await CreateOrgAsync(client);

        var response = await SendPostWithCsrfAsync(client, "/billing/checkout", new
        {
            orgId,
            planId = "pro",
            successUrl = "https://evil.com/steal",
            cancelUrl = "/en/billing/cancel"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCheckout_WithProtocolRelativeUrl_ShouldReturnValidationError()
    {
        var client = CreateClient();
        await RegisterUserAsync(client);
        var orgId = await CreateOrgAsync(client);

        var response = await SendPostWithCsrfAsync(client, "/billing/checkout", new
        {
            orgId,
            planId = "pro",
            successUrl = "//evil.com",
            cancelUrl = "/en/billing/cancel"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCheckout_WhenNotOrgMember_ShouldReturn403()
    {
        var clientA = CreateClient();
        await RegisterUserAsync(clientA);
        var orgId = await CreateOrgAsync(clientA);

        var clientB = CreateClient();
        await RegisterUserAsync(clientB);

        var response = await SendPostWithCsrfAsync(clientB, "/billing/checkout", new
        {
            orgId,
            planId = "pro",
            successUrl = "/en/billing/success",
            cancelUrl = "/en/billing/cancel"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCheckout_WithValidRequest_ShouldReturn200()
    {
        var client = CreateClient();
        await RegisterUserAsync(client);
        var orgId = await CreateOrgAsync(client);

        var response = await SendPostWithCsrfAsync(client, "/billing/checkout", new
        {
            orgId,
            planId = "pro",
            successUrl = "/en/billing/success",
            cancelUrl = "/en/billing/cancel"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("checkoutUrl").GetString().Should().StartWith("https://checkout.stripe.com/");
    }

    // ───────── POST /billing/customer-portal ─────────

    [Fact]
    public async Task CreateCustomerPortal_WhenNotAuthenticated_ShouldReturn401()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/billing/customer-portal", new
        {
            orgId = Guid.NewGuid(),
            returnUrl = "/en/billing"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCustomerPortal_WithoutCsrf_ShouldReturn403()
    {
        var client = CreateClient();
        await RegisterUserAsync(client);

        var response = await client.PostAsJsonAsync("/billing/customer-portal", new
        {
            orgId = Guid.NewGuid(),
            returnUrl = "/en/billing"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCustomerPortal_WithAbsoluteReturnUrl_ShouldReturnValidationError()
    {
        var client = CreateClient();
        await RegisterUserAsync(client);
        var orgId = await CreateOrgAsync(client);

        var response = await SendPostWithCsrfAsync(client, "/billing/customer-portal", new
        {
            orgId,
            returnUrl = "https://evil.com"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCustomerPortal_WhenNotOrgMember_ShouldReturn403()
    {
        var clientA = CreateClient();
        await RegisterUserAsync(clientA);
        var orgId = await CreateOrgAsync(clientA);

        var clientB = CreateClient();
        await RegisterUserAsync(clientB);

        var response = await SendPostWithCsrfAsync(clientB, "/billing/customer-portal", new
        {
            orgId,
            returnUrl = "/en/billing"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCustomerPortal_WithValidRequest_ShouldReturn200()
    {
        var client = CreateClient();
        await RegisterUserAsync(client);
        var orgId = await CreateOrgAsync(client);

        var response = await SendPostWithCsrfAsync(client, "/billing/customer-portal", new
        {
            orgId,
            returnUrl = "/en/billing"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("portalUrl").GetString().Should().StartWith("https://billing.stripe.com/");
    }

    // ───────── POST /billing/webhook ─────────

    [Fact]
    public async Task HandleWebhook_WithInvalidSignature_ShouldReturn400()
    {
        var client = CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/billing/webhook")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Stripe-Signature", "t=12345,v1=invalid_signature");

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
