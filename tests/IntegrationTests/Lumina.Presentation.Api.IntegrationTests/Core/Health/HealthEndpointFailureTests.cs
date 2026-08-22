#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Health;

/// <summary>
/// Contains integration tests for the <c>/health/ready</c> probe route when the database is unreachable.
/// </summary>
[ExcludeFromCodeCoverage]
public class HealthEndpointFailureTests : IClassFixture<FailingDatabaseApiFactory>
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthEndpointFailureTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory with a failing database health check.</param>
    public HealthEndpointFailureTests(FailingDatabaseApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task HealthReady_WhenDatabaseIsUnreachable_ShouldReturnServiceUnavailable()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}
