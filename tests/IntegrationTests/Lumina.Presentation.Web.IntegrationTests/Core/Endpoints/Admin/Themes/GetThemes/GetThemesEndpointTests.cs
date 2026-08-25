#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.GetThemes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Themes.GetThemes;

/// <summary>
/// Contains integration tests for the <c>/{culture}/admin/themes/api-get-themes</c> route served by the <see cref="GetThemesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemesEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();
    private readonly ThemeSettingsResponseDtoFixture _themeSettingsResponseDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetThemesEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetThemes_WhenCalledByAuthenticatedAdmin_ShouldReturnThemesFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
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
