#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Common.Documentation;

/// <summary>
/// Contains integration tests for the OpenAPI and Scalar documentation endpoints of the Web application.
/// </summary>
[ExcludeFromCodeCoverage]
public class OpenApiEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenApiEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public OpenApiEndpointTests(LuminaWebFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task OpenApi_WhenRequested_ShouldServeDocument()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/openapi/v1.json");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.Contains("openapi", content);
    }

    [Fact]
    public async Task Scalar_WhenRequested_ShouldServeReferencePage()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/scalar/v1");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
    }
}
