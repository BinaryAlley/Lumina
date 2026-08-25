#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Home;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Home;

/// <summary>
/// Contains security tests for the <c>/{culture}</c> and <c>/</c> routes served by the <see cref="HomeIndexEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class HomeIndexEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeIndexEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public HomeIndexEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task Home_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us");

        // Assert
        // the home page requires authentication, so an anonymous request must be redirected to the login page
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Exception", content, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Home_WhenCalledWithoutCulture_ShouldRedirectToDefaultCulture()
    {
        // Arrange
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/");

        // Assert
        // the culture-less root must never demand authentication or leak internals, only redirect to the default culture
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/en-us/", response.Headers.Location!.ToString());
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Exception", content, System.StringComparison.OrdinalIgnoreCase);
    }
}
