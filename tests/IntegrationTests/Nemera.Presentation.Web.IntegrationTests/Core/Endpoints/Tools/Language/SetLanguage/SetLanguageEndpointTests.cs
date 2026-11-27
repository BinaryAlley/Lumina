#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.Tools.Language.SetLanguage;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Tools.Language.SetLanguage;

/// <summary>
/// Contains integration tests for the <see cref="SetLanguageEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLanguageEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLanguageEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public SetLanguageEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task SetLanguage_WhenCalledByAuthenticatedUser_ShouldSetCultureCookieAndRedirectWithNewCulture()
    {
        // Arrange
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        string returnUrl = Uri.EscapeDataString("/en-us/tools/settings");

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/tools/language/set-language?newCulture=de-DE&returnUrl={returnUrl}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/de-de/tools/settings", response.Headers.Location!.ToString());
        Assert.Contains("de-DE", response.Headers.GetValues("Set-Cookie").First());
    }

    [Fact]
    public async Task SetLanguage_WhenCalledByAuthenticatedUserWithoutReturnUrl_ShouldRedirectToHomePageWithNewCulture()
    {
        // Arrange
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/tools/language/set-language?newCulture=de-DE");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/de-de", response.Headers.Location!.ToString());
        Assert.Contains("de-DE", response.Headers.GetValues("Set-Cookie").First());
    }

    [Fact]
    public async Task SetLanguage_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us/tools/language/set-language?newCulture=de-DE");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
    }
}
