#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.Enums.Authorization;
using Nemera.Presentation.Web.Core.Endpoints.Admin.Themes.RestoreTheme;
using Nemera.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Admin.Themes.RestoreTheme;

/// <summary>
/// Contains security tests for the <see cref="RestoreThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreThemeEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public RestoreThemeEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task RestoreTheme_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage restoreRequest = CreateRestoreRequest("test-theme");
        restoreRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(restoreRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "themes/test-theme/restore");
    }

    [Fact]
    public async Task RestoreTheme_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: Enum.GetValues<AuthorizationPermission>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage restoreRequest = CreateRestoreRequest("test-theme");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(restoreRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "themes/test-theme/restore");
    }

    [Fact]
    public async Task RestoreTheme_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: Enum.GetValues<AuthorizationPermission>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage restoreRequest = CreateRestoreRequest("test-theme");
        restoreRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(restoreRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/en-us/auth/access-denied", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "themes/test-theme/restore");
    }

    /// <summary>
    /// Builds a POST request for the theme restore route with the JSON content type required by the antiforgery validation.
    /// </summary>
    /// <param name="themeId">The manifest id of the bundled theme to restore.</param>
    /// <returns>The configured POST request.</returns>
    private static HttpRequestMessage CreateRestoreRequest(string themeId)
    {
        HttpRequestMessage restoreRequest = new(HttpMethod.Post, $"/en-us/admin/themes/api-restore-theme/{themeId}")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        restoreRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        restoreRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return restoreRequest;
    }
}
