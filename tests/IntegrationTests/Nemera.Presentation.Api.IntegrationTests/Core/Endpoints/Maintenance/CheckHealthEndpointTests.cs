#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Api.Core.Endpoints.Maintenance;
using Nemera.Presentation.Api.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.IntegrationTests.Core.Endpoints.Maintenance;

/// <summary>
/// Contains integration tests for the <see cref="CheckHealthEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckHealthEndpointTests : IClassFixture<AuthenticatedNemeraApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedNemeraApiFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckHealthEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public CheckHealthEndpointTests(AuthenticatedNemeraApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task CheckHealth_WhenCalled_ShouldReturnOk()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/check-health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
