#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Themes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Themes;

/// <summary>
/// Contains integration tests for the <c>/{culture}/admin/manage-themes</c> route served by the <see cref="ThemesIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemesIndexViewEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemesIndexViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ThemesIndexViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ThemesIndex_WhenCalledByAuthenticatedAdmin_ShouldRenderThemesManagementPage()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
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
