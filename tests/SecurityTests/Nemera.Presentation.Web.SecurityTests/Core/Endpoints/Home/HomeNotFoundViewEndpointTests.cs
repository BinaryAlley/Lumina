#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.Home;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Home;

/// <summary>
/// Contains security tests for the <see cref="HomeNotFoundViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class HomeNotFoundViewEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeNotFoundViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public HomeNotFoundViewEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task HomeNotFoundView_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us/not-found");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
    }
}
