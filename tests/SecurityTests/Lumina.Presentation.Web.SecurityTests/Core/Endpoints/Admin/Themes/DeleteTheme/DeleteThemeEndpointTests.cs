#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.DeleteTheme;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Admin.Themes.DeleteTheme;

/// <summary>
/// Contains security tests for the <c>/{culture}/admin/themes/api-delete-theme/{themeId}</c> route served by the <see cref="DeleteThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteThemeEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public DeleteThemeEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task DeleteTheme_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage deleteRequest = CreateDeleteRequest("test-theme");
        deleteRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.DeleteEndpointsCalled, endpoint => endpoint == "themes/test-theme");
    }

    [Fact]
    public async Task DeleteTheme_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest("test-theme");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.DeleteEndpointsCalled, endpoint => endpoint == "themes/test-theme");
    }

    /// <summary>
    /// Builds a DELETE request for the theme deletion route with the JSON content type required by the antiforgery validation.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme to delete.</param>
    /// <returns>The configured DELETE request.</returns>
    private static HttpRequestMessage CreateDeleteRequest(string themeId)
    {
        HttpRequestMessage deleteRequest = new(HttpMethod.Delete, $"/en-us/admin/themes/api-delete-theme/{themeId}")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        deleteRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        deleteRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return deleteRequest;
    }
}
