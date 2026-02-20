// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Api.Services;
using FluentAssertions;

namespace Propely.OrgsApi.UnitTests.Api.Services;

public sealed class CsrfValidatorTests
{
    private const string TestSecret = "test-secret-key-that-is-long-enough-for-hmac";

    [Fact]
    public void GenerateToken_ShouldReturnThreePartToken()
    {
        // Act
        var token = CsrfValidator.GenerateToken(TestSecret);

        // Assert
        var parts = token.Split('.');
        parts.Should().HaveCount(3, "token format is {hex-timestamp}.{random-hex}.{hmac-hex}");
    }

    [Fact]
    public void GenerateToken_ShouldProduceValidHmac()
    {
        // Act
        var token = CsrfValidator.GenerateToken(TestSecret);

        // Assert
        var lastDot = token.LastIndexOf('.');
        var payload = token[..lastDot];
        var signature = token[(lastDot + 1)..];
        var expectedSignature = CsrfValidator.ComputeHmac(payload, TestSecret);
        signature.Should().Be(expectedSignature);
    }

    [Fact]
    public void GenerateToken_ShouldProduceUniqueTokens()
    {
        // Act
        var token1 = CsrfValidator.GenerateToken(TestSecret);
        var token2 = CsrfValidator.GenerateToken(TestSecret);

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void ComputeHmac_ShouldReturnConsistentResult()
    {
        // Arrange
        var payload = "test-payload";

        // Act
        var hmac1 = CsrfValidator.ComputeHmac(payload, TestSecret);
        var hmac2 = CsrfValidator.ComputeHmac(payload, TestSecret);

        // Assert
        hmac1.Should().Be(hmac2);
    }

    [Fact]
    public void ComputeHmac_ShouldDifferWithDifferentSecrets()
    {
        // Arrange
        var payload = "test-payload";

        // Act
        var hmac1 = CsrfValidator.ComputeHmac(payload, "secret-one");
        var hmac2 = CsrfValidator.ComputeHmac(payload, "secret-two");

        // Assert
        hmac1.Should().NotBe(hmac2);
    }

    [Fact]
    public void ComputeHmac_ShouldDifferWithDifferentPayloads()
    {
        // Act
        var hmac1 = CsrfValidator.ComputeHmac("payload-one", TestSecret);
        var hmac2 = CsrfValidator.ComputeHmac("payload-two", TestSecret);

        // Assert
        hmac1.Should().NotBe(hmac2);
    }

    [Fact]
    public void GenerateToken_ShouldContainHexTimestamp()
    {
        // Act
        var token = CsrfValidator.GenerateToken(TestSecret);

        // Assert
        var firstDot = token.IndexOf('.');
        var timestampHex = token[..firstDot];
        var parsed = long.TryParse(
            timestampHex,
            System.Globalization.NumberStyles.HexNumber,
            null,
            out var unixSeconds);
        parsed.Should().BeTrue("first segment should be a hex timestamp");

        var tokenTime = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        tokenTime.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }
}
