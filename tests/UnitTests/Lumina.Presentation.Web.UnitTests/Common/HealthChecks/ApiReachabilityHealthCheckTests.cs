#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.HealthChecks;
using Lumina.Presentation.Web.Fixtures.Common.Api;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.HealthChecks;

/// <summary>
/// Contains unit tests for the <see cref="ApiReachabilityHealthCheck"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ApiReachabilityHealthCheckTests
{
    private readonly ServerConfigurationDtoFixture _serverConfigurationDtoFixture = new();

    [Fact]
    public async Task CheckHealthAsync_WhenApiReturnsSuccess_ShouldReturnHealthyAndProbeReadyEndpoint()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        ApiReachabilityHealthCheck sut = CreateSut(messageHandler);

        // Act
        HealthCheckResult result = await sut.CheckHealthAsync(CreateHealthCheckContext(sut), CancellationToken.None);

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Single(messageHandler.Requests);
        Assert.Equal("http://localhost:5214/health/ready", messageHandler.Requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task CheckHealthAsync_WhenApiReturnsErrorStatus_ShouldReturnUnhealthyWithStatusCodeDescription()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        ApiReachabilityHealthCheck sut = CreateSut(messageHandler);

        // Act
        HealthCheckResult result = await sut.CheckHealthAsync(CreateHealthCheckContext(sut), CancellationToken.None);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Equal("The API returned status 503.", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenApiRequestThrowsException_ShouldReturnUnhealthyWithException()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => throw new HttpRequestException("The API is down."));
        ApiReachabilityHealthCheck sut = CreateSut(messageHandler);

        // Act
        HealthCheckResult result = await sut.CheckHealthAsync(CreateHealthCheckContext(sut), CancellationToken.None);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Equal("The API is not reachable.", result.Description);
        Assert.NotNull(result.Exception);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenCancellationIsRequestedBeforeProbe_ShouldReturnUnhealthy()
    {
        // Arrange
        TestApiHttpMessageHandler messageHandler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        ApiReachabilityHealthCheck sut = CreateSut(messageHandler);
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        HealthCheckResult result = await sut.CheckHealthAsync(CreateHealthCheckContext(sut), cancellationTokenSource.Token);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Equal("The API is not reachable.", result.Description);
    }

    /// <summary>
    /// Creates a <see cref="HealthCheckContext"/> whose registration points at the given health check.
    /// </summary>
    /// <param name="healthCheck">The health check to register in the context.</param>
    /// <returns>The created <see cref="HealthCheckContext"/> instance.</returns>
    private static HealthCheckContext CreateHealthCheckContext(IHealthCheck healthCheck)
    {
        // the ApiReachabilityHealthCheck never reads the registration, so a bare context is sufficient
        return new HealthCheckContext();
    }

    /// <summary>
    /// Creates the system under test configured with the provided message handler and a fixed server configuration.
    /// </summary>
    /// <param name="messageHandler">The message handler backing the inner <see cref="HttpClient"/>.</param>
    /// <returns>The created <see cref="ApiReachabilityHealthCheck"/>.</returns>
    private ApiReachabilityHealthCheck CreateSut(TestApiHttpMessageHandler messageHandler)
    {
        HttpClient httpClient = new(messageHandler);
        IHttpClientFactory httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient().Returns(httpClient);
        IOptionsSnapshot<ServerConfigurationDto> serverConfigurationOptions = Substitute.For<IOptionsSnapshot<ServerConfigurationDto>>();
        serverConfigurationOptions.Value.Returns(_serverConfigurationDtoFixture.Create(apiVersion: '1', baseAddress: "http://localhost", port: 5214));
        return new ApiReachabilityHealthCheck(httpClientFactory, serverConfigurationOptions);
    }
}
