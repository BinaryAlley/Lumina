#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.Home;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Home;

/// <summary>
/// Contains integration tests for the <see cref="HomeErrorEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class HomeErrorEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeErrorEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public HomeErrorEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task HomeError_WhenCalledByAuthenticatedUser_ShouldRenderErrorPage()
    {
        // Arrange
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/error");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        string content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Navigation failed!", content);
        Assert.Contains("data-server-unreachable-banner", content);
    }
}
