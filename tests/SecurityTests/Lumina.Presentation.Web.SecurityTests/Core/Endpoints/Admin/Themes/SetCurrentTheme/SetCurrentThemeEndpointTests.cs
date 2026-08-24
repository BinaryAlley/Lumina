#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Themes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.SetCurrentTheme;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Admin.Themes.SetCurrentTheme;

/// <summary>
/// Contains security tests for the <c>/{culture}/admin/themes/api-set-current-theme</c> route served by the <see cref="SetCurrentThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetCurrentThemeEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetCurrentThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public SetCurrentThemeEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task SetCurrentTheme_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage setCurrentThemeRequest = CreateSetCurrentThemeRequest();
        setCurrentThemeRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(setCurrentThemeRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == "themes/current");
    }

    [Fact]
    public async Task SetCurrentTheme_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage setCurrentThemeRequest = CreateSetCurrentThemeRequest();

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(setCurrentThemeRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == "themes/current");
    }

    /// <summary>
    /// Builds a PUT request for the set current theme route with the JSON content type required by the antiforgery validation.
    /// </summary>
    /// <returns>The configured PUT request.</returns>
    private static HttpRequestMessage CreateSetCurrentThemeRequest()
    {
        HttpRequestMessage setCurrentThemeRequest = new(HttpMethod.Put, "/en-us/admin/themes/api-set-current-theme")
        {
            Content = JsonContent.Create(new SetCurrentThemeRequest("test-theme"))
        };
        setCurrentThemeRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        setCurrentThemeRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return setCurrentThemeRequest;
    }
}
