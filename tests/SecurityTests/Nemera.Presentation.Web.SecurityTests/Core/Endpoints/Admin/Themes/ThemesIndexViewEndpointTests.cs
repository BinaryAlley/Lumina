#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.Themes;
using Nemera.Presentation.Web.Common.Enums.Authorization;
using Nemera.Presentation.Web.Core.Endpoints.Admin.Themes;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Themes;
using Nemera.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Admin.Themes;

/// <summary>
/// Contains security tests for the <see cref="ThemesIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemesIndexViewEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemesIndexViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ThemesIndexViewEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ThemesIndex_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us/admin/manage-themes");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "themes");
    }

    [Fact]
    public async Task ThemesIndex_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: Enum.GetValues<AuthorizationPermission>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/admin/manage-themes");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/en-us/auth/access-denied", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "themes");
    }

    [Fact]
    public async Task ThemesIndex_WhenCalledByAdminUser_ShouldReturnThemesPage()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: Enum.GetValues<AuthorizationPermission>());
        ThemeResponseDto[] expectedThemes = [.. _themeResponseDtoFixture.CreateMany()];
        ThemeResponseDto currentTheme = _themeResponseDtoFixture.Create(themeId: "current-theme");
        _apiFactory.ApiClientStub.RegisterGetResponse("themes", expectedThemes);
        _apiFactory.ApiClientStub.RegisterGetResponse("themes/current", currentTheme);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/admin/manage-themes");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "themes");
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "themes/current");
    }
}
