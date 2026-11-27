#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Health;

/// <summary>
/// Contains integration tests for the <c>/health/live</c> and <c>/health/ready</c> probe routes.
/// </summary>
[ExcludeFromCodeCoverage]
public class HealthEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public HealthEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task HealthLive_WhenCalled_ShouldReturnOk()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthReady_WhenDatabaseIsReachable_ShouldReturnHealthyReport()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/health/ready");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        HealthReportDto report = JsonSerializer.Deserialize<HealthReportDto>(content, _jsonOptions)!;
        Assert.Equal("Healthy", report.Status);
        HealthCheckReportDto databaseCheck = Assert.Single(report.Checks, check => check.Name == "database");
        Assert.Equal("Healthy", databaseCheck.Status);
    }

    /// <summary>
    /// Deserialized shape of the health report JSON written by the API.
    /// </summary>
    private sealed record HealthReportDto(string Status, List<HealthCheckReportDto> Checks);

    /// <summary>
    /// Deserialized shape of a single health check entry within the health report.
    /// </summary>
    private sealed record HealthCheckReportDto(string Name, string Status, string? Description);
}
