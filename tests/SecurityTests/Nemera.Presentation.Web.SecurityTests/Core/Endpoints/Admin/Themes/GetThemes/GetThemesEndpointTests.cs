#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.Themes;
using Nemera.Presentation.Web.Common.Enums.Authorization;
using Nemera.Presentation.Web.Core.Endpoints.Admin.Themes.GetThemes;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Themes;
using Nemera.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Admin.Themes.GetThemes;

/// <summary>
/// Contains security tests for the <see cref="GetThemesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemesEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();
    private readonly ThemeSettingsResponseDtoFixture _themeSettingsResponseDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetThemesEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetThemes_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/admin/themes/api-get-themes");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "themes");
    }

    [Fact]
    public async Task GetThemes_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: Enum.GetValues<AuthorizationPermission>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/admin/themes/api-get-themes");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/en-us/auth/access-denied", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "themes");
    }

    [Fact]
    public async Task GetThemes_WhenCalledByAdminUser_ShouldReturnThemesFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: Enum.GetValues<AuthorizationPermission>());
        ThemeResponseDto[] expectedThemes = [.. _themeResponseDtoFixture.CreateMany()];
        ThemeResponseDto currentTheme = _themeResponseDtoFixture.Create(themeId: "current-theme");
        ThemeSettingsResponseDto expectedSettings = _themeSettingsResponseDtoFixture.Create();
        _apiFactory.ApiClientStub.RegisterGetResponse("themes", expectedThemes);
        _apiFactory.ApiClientStub.RegisterGetResponse("themes/current", currentTheme);
        _apiFactory.ApiClientStub.RegisterGetResponse("themes/settings", expectedSettings);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/admin/themes/api-get-themes");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(expectedThemes.Length, data.GetProperty("themes").GetArrayLength());
        Assert.Equal("current-theme", data.GetProperty("currentThemeId").GetString());
        Assert.Equal(expectedSettings.MaxArchiveBytes, data.GetProperty("maxArchiveBytes").GetInt64());
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "themes");
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "themes/current");
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "themes/settings");
    }
}
