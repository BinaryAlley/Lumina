#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.Themes;
using Nemera.Presentation.Web.Core.Endpoints.Admin.Themes;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Themes;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Themes;

/// <summary>
/// Contains integration tests for the <see cref="ThemesIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemesIndexViewEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
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
