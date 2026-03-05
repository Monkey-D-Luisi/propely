// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.PropertiesApi.Client;

namespace Propely.PropertiesApi.UnitTests.Client;

public class PropertiesApiClientOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var options = new PropertiesApiClientOptions();

        options.BaseUrl.Should().Be("http://localhost:5030");
        options.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        options.RetryCount.Should().Be(3);
        options.CircuitBreakerFailureThreshold.Should().Be(5);
        options.CircuitBreakerDuration.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var options = new PropertiesApiClientOptions
        {
            BaseUrl = "http://custom:8080",
            Timeout = TimeSpan.FromSeconds(60),
            RetryCount = 5,
            CircuitBreakerFailureThreshold = 10,
            CircuitBreakerDuration = TimeSpan.FromMinutes(1)
        };

        options.BaseUrl.Should().Be("http://custom:8080");
        options.Timeout.Should().Be(TimeSpan.FromSeconds(60));
        options.RetryCount.Should().Be(5);
        options.CircuitBreakerFailureThreshold.Should().Be(10);
        options.CircuitBreakerDuration.Should().Be(TimeSpan.FromMinutes(1));
    }
}
