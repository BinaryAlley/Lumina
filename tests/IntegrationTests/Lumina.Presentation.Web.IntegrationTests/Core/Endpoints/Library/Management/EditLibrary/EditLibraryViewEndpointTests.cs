#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.EditLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.EditLibrary;

/// <summary>
/// Contains integration tests for the <c>/{culture}/libraries/manage/item/{{id}}</c> route served by the <see cref="EditLibraryViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditLibraryViewEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();
    private readonly ThemeTemplateResponseDtoFixture _themeTemplateResponseDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EditLibraryViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public EditLibraryViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task EditLibraryView_WhenCalledByAuthenticatedUser_ShouldRenderLibraryEditingView()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        LibraryDto expectedLibrary = _libraryDtoFixture.Create(id: libraryId, title: "Books", libraryType: "Book");
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}", expectedLibrary);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/libraries/manage/item/{libraryId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains($"libraries/{libraryId}", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task EditLibraryView_WhenThemeProvidesFileSystemBrowserTemplates_ShouldRenderThemedComponent()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        LibraryDto expectedLibrary = _libraryDtoFixture.Create(id: libraryId, title: "Books", libraryType: "Book");
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}", expectedLibrary);
        ThemeResponseDto theme = _themeResponseDtoFixture.Create(themeId: "lumina-default");
        _apiFactory.ApiClientStub.RegisterGetResponse(ApiRoutes.Themes.GET_CURRENT_THEME, theme);
        RegisterThemeTemplate(theme, "shared/file-system-browser", "<script type=\"text/plain\" id=\"fsb-template-tree-node\">{{{treeNodeTemplate}}}</script><div id=\"file-system-browser-dialog\">themed</div>");
        RegisterThemeTemplate(theme, "shared/file-system-browser/tree-node", "<div class=\"tree-node\">{{name}}</div>");
        RegisterThemeTemplate(theme, "shared/file-system-browser/explorer-item", "<div class=\"e\">{{name}}</div>");
        RegisterThemeTemplate(theme, "shared/file-system-browser/path-segment", "<li>{{path}}</li>");
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/libraries/manage/item/{libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains("id=\"fsb-template-tree-node\"", content);
        Assert.Contains("initFileSystemBrowser(", content);
        Assert.Contains("themed", content);
    }

    [Fact]
    public async Task EditLibraryView_WhenThemeDoesNotProvideFileSystemBrowserTemplate_ShouldRenderApplicationFallbackComponent()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        LibraryDto expectedLibrary = _libraryDtoFixture.Create(id: libraryId, title: "Books", libraryType: "Book");
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}", expectedLibrary);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/libraries/manage/item/{libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains("id=\"file-system-browser-dialog\"", content);
        Assert.DoesNotContain("fsb-template-tree-node", content);
        Assert.Contains("initFileSystemBrowser(", content);
    }

    /// <summary>
    /// Registers the theme template response returned for the specified page key of the given theme.
    /// </summary>
    /// <param name="theme">The theme the template belongs to.</param>
    /// <param name="pageKey">The page key of the template.</param>
    /// <param name="template">The template source returned for the document.</param>
    private void RegisterThemeTemplate(ThemeResponseDto theme, string pageKey, string template)
    {
        string endpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
            .Replace("{themeId}", theme.ThemeId)
            .Replace("{*pageKey}", pageKey);
        _apiFactory.ApiClientStub.RegisterGetResponse(endpoint, _themeTemplateResponseDtoFixture.Create(theme: theme, template: template));
    }
}
