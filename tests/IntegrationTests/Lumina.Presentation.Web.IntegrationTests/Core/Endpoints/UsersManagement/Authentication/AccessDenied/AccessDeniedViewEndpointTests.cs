#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.AccessDenied;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.UsersManagement.Authentication.AccessDenied;

/// <summary>
/// Contains integration tests for the <c>/{culture}/auth/access-denied</c> route served by the <see cref="AccessDeniedViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AccessDeniedViewEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccessDeniedViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public AccessDeniedViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task AccessDeniedView_WhenCalled_ShouldRenderAccessDeniedPage()
    {
        // Arrange
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us/auth/access-denied");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
    }
}
