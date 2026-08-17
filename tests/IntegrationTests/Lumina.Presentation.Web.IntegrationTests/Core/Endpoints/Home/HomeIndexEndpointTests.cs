#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Home;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Home;

/// <summary>
/// Contains integration tests for the <c>/{culture}</c> and <c>/</c> routes served by the <see cref="HomeIndexEndpoint"/> class.
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
    public async Task Home_WhenCalledWithoutCulture_ShouldRedirectToDefaultCulture()
    {
        // Arrange
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/en-us/", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Home_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Home_WhenCalledByAuthenticatedUser_ShouldReturnHomePage()
    {
        // Arrange
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        string content = await response.Content.ReadAsStringAsync();
        Assert.Contains("html", content);
    }

    [Theory]
    [InlineData("/de-de")] // German culture
    [InlineData("/fr-fr")] // French culture
    [InlineData("/ja-jp")] // Japanese culture
    public async Task Home_WhenCalledByAuthenticatedUserWithLocalizedCulture_ShouldReturnHomePage(string culturePath)
    {
        // Arrange
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(culturePath);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
