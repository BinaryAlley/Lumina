#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Health;

/// <summary>
/// Contains integration tests for the <c>/health/ready</c> probe route when the API is unreachable.
/// </summary>
[ExcludeFromCodeCoverage]
public class HealthEndpointFailureTests : IClassFixture<FailingApiHealthFactory>
{
    private readonly FailingApiHealthFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthEndpointFailureTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory with a failing API reachability check.</param>
    public HealthEndpointFailureTests(FailingApiHealthFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task HealthReady_WhenApiIsUnreachable_ShouldReturnServiceUnavailable()
    {
        // Arrange
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}
