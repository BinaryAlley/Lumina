#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Api.Core.Endpoints.Library.Management.CancelLibrariesScan;
using Nemera.Presentation.Api.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.IntegrationTests.Core.Endpoints.Library.Management.CancelLibrariesScan;

/// <summary>
/// Contains integration tests for the <see cref="CancelLibrariesScanEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibrariesScanEndpointTests : IClassFixture<AuthenticatedNemeraApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedNemeraApiFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibrariesScanEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public CancelLibrariesScanEndpointTests(AuthenticatedNemeraApiFactory apiFactory)
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
    public async Task CancelLibrariesScan_WhenCalled_ShouldReturnNoContent()
    {
        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/libraries/scans/cancel", new { });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
