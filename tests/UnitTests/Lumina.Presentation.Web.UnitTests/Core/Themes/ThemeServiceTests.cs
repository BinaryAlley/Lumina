#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Enums.Themes;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemeService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeServiceTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly ThemeService _sut;
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();
    private readonly ThemeSettingsResponseDtoFixture _themeSettingsResponseDtoFixture = new();
    private readonly ThemeTemplateResponseDtoFixture _themeTemplateResponseDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeServiceTests"/> class.
    /// </summary>
    public ThemeServiceTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = new ThemeService(_mockApiHttpClient);
    }

    [Fact]
    public async Task GetThemesAsync_WhenApiReturnsThemes_ShouldReturnMappedThemeInfoList()
    {
        // Arrange
        ThemeResponseDto[] themes =
        [
            _themeResponseDtoFixture.Create(themeId: "editorial-paper", name: "Editorial Paper", previewPath: "preview.png", includePreviewPath: true, installSource: ThemeInstallSource.Bundled),
            _themeResponseDtoFixture.Create(themeId: "lumina-default", name: "Lumina Default", previewPath: "preview.png", includePreviewPath: true, installSource: ThemeInstallSource.Uploaded)
        ];
        _mockApiHttpClient.GetAsync<ThemeResponseDto[]>(ApiRoutes.Themes.GET_THEMES, Arg.Any<CancellationToken>())
            .Returns(themes);

        // Act
        IReadOnlyList<ThemeInfoDto> result = await _sut.GetThemesAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("editorial-paper", result[0].Id);
        Assert.Equal("Editorial Paper", result[0].Name);
        Assert.Equal(themes[0].Description, result[0].Description);
        Assert.Equal(themes[0].Author, result[0].Author);
        Assert.Equal(themes[0].Version, result[0].Version);
        Assert.Equal("/theme-assets/editorial-paper/preview.png", result[0].PreviewUrl);
        Assert.True(result[0].IsBundled);
        Assert.False(result[1].IsBundled);
        await _mockApiHttpClient.Received(1).GetAsync<ThemeResponseDto[]>(ApiRoutes.Themes.GET_THEMES, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetThemesAsync_WhenThemeHasNoPreviewPath_ShouldReturnPlaceholderPreviewUrl()
    {
        // Arrange
        ThemeResponseDto theme = _themeResponseDtoFixture.Create(themeId: "editorial-paper", previewPath: null);
        _mockApiHttpClient.GetAsync<ThemeResponseDto[]>(ApiRoutes.Themes.GET_THEMES, Arg.Any<CancellationToken>())
            .Returns([theme]);

        // Act
        IReadOnlyList<ThemeInfoDto> result = await _sut.GetThemesAsync(CancellationToken.None);

        // Assert
        Assert.Equal("/admin/theme-placeholder.svg", result[0].PreviewUrl);
    }

    [Fact]
    public async Task GetCurrentThemeAsync_WhenApiReturnsCurrentTheme_ShouldReturnMappedThemeInfo()
    {
        // Arrange
        ThemeResponseDto theme = _themeResponseDtoFixture.Create(themeId: "editorial-paper", previewPath: "preview.png", includePreviewPath: true);
        _mockApiHttpClient.GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>())
            .Returns(theme);

        // Act
        ThemeInfoDto result = await _sut.GetCurrentThemeAsync(CancellationToken.None);

        // Assert
        Assert.Equal("editorial-paper", result.Id);
        Assert.Equal(theme.Name, result.Name);
        Assert.Equal("/theme-assets/editorial-paper/preview.png", result.PreviewUrl);
        await _mockApiHttpClient.Received(1).GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetThemeSettingsAsync_WhenApiReturnsSettings_ShouldReturnSettings()
    {
        // Arrange
        ThemeSettingsResponseDto settings = _themeSettingsResponseDtoFixture.Create(maxArchiveBytes: 10 * 1024 * 1024, allowThemeScripts: true);
        _mockApiHttpClient.GetAsync<ThemeSettingsResponseDto>(ApiRoutes.Themes.GET_THEME_SETTINGS, Arg.Any<CancellationToken>())
            .Returns(settings);

        // Act
        ThemeSettingsResponseDto result = await _sut.GetThemeSettingsAsync(CancellationToken.None);

        // Assert
        Assert.Equal(settings, result);
        await _mockApiHttpClient.Received(1).GetAsync<ThemeSettingsResponseDto>(ApiRoutes.Themes.GET_THEME_SETTINGS, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetCurrentThemeAsync_WhenCalled_ShouldSendThemeIdToApi()
    {
        // Arrange
        string themeId = "editorial-paper";
        _mockApiHttpClient.PutAsync<ThemeResponseDto, object>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(_themeResponseDtoFixture.Create());

        // Act
        await _sut.SetCurrentThemeAsync(themeId, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<ThemeResponseDto, object>(
            ApiRoutes.Themes.SET_CURRENT_THEME,
            Arg.Is<object>(payload => payload.GetType().GetProperty("ThemeId")!.GetValue(payload)!.ToString()! == themeId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InstallAsync_WhenCalled_ShouldUploadArchiveToInstallEndpoint()
    {
        // Arrange
        ThemeResponseDto installedTheme = _themeResponseDtoFixture.Create(themeId: "installed-theme");
        using (MemoryStream archiveStream = new([1, 2, 3, 4]))
        {
            string fileName = "theme-pack.zip";
            _mockApiHttpClient.PostMultipartAsync<ThemeResponseDto>(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(installedTheme);

            // Act
            ThemeInfoDto result = await _sut.InstallAsync(archiveStream, fileName, CancellationToken.None);

            // Assert
            Assert.Equal("installed-theme", result.Id);
            await _mockApiHttpClient.Received(1).PostMultipartAsync<ThemeResponseDto>(
                ApiRoutes.Themes.INSTALL_THEME,
                archiveStream,
                fileName,
                "archive",
                Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task DeleteThemeAsync_WhenCalled_ShouldDeleteThemeWithResolvedEndpoint()
    {
        // Arrange
        string themeId = "editorial-paper";
        string expectedEndpoint = ApiRoutes.Themes.DELETE_THEME.Replace("{themeId}", themeId);

        // Act
        await _sut.DeleteThemeAsync(themeId, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(expectedEndpoint, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRenderDocumentAsync_WhenThemeIdProvided_ShouldFetchTemplateWithResolvedRoute()
    {
        // Arrange
        string themeId = "editorial-paper";
        string pageKey = "shared/layout";
        ThemeTemplateResponseDto templateResponse = _themeTemplateResponseDtoFixture.Create(theme: _themeResponseDtoFixture.Create(themeId: themeId), template: "Hello {{title}}");
        string expectedEndpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
            .Replace("{themeId}", themeId)
            .Replace("{*pageKey}", pageKey);
        _mockApiHttpClient.GetAsync<ThemeTemplateResponseDto>(expectedEndpoint, Arg.Any<CancellationToken>())
            .Returns(templateResponse);

        // Act
        ThemeRenderDocumentDto result = await _sut.GetRenderDocumentAsync(pageKey, themeId, CancellationToken.None);

        // Assert
        Assert.Equal(themeId, result.Theme.Id);
        Assert.Equal("Hello {{title}}", result.Template);
        await _mockApiHttpClient.DidNotReceive().GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>());
        await _mockApiHttpClient.Received(1).GetAsync<ThemeTemplateResponseDto>(expectedEndpoint, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRenderDocumentAsync_WhenThemeIdMissing_ShouldResolveCurrentThemeFirst()
    {
        // Arrange
        string pageKey = "home/index";
        ThemeResponseDto currentTheme = _themeResponseDtoFixture.Create(themeId: "lumina-default");
        ThemeTemplateResponseDto templateResponse = _themeTemplateResponseDtoFixture.Create(theme: _themeResponseDtoFixture.Create(themeId: "lumina-default"), template: "Home");
        string expectedEndpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
            .Replace("{themeId}", "lumina-default")
            .Replace("{*pageKey}", pageKey);
        _mockApiHttpClient.GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>())
            .Returns(currentTheme);
        _mockApiHttpClient.GetAsync<ThemeTemplateResponseDto>(expectedEndpoint, Arg.Any<CancellationToken>())
            .Returns(templateResponse);

        // Act
        ThemeRenderDocumentDto result = await _sut.GetRenderDocumentAsync(pageKey, null, CancellationToken.None);

        // Assert
        Assert.Equal("lumina-default", result.Theme.Id);
        await _mockApiHttpClient.Received(1).GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>());
        await _mockApiHttpClient.Received(1).GetAsync<ThemeTemplateResponseDto>(expectedEndpoint, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BuildArchiveAsync_WhenApiReturnsBlob_ShouldReturnZipArchiveWithResolvedFileName()
    {
        // Arrange
        string themeId = "editorial-paper";
        byte[] data = [0x50, 0x4B, 0x03, 0x04];
        _mockApiHttpClient.GetBlobAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new BlobDataDto { Data = data, ContentType = "application/zip" });
        string expectedEndpoint = ApiRoutes.Themes.GET_THEME_ARCHIVE.Replace("{themeId}", themeId);

        // Act
        ThemeArchiveDto result = await _sut.BuildArchiveAsync(themeId, CancellationToken.None);

        // Assert
        Assert.Equal($"{themeId}.zip", result.FileName);
        using (MemoryStream content = new())
        {
            await result.Content.CopyToAsync(content);
            Assert.Equal(data, content.ToArray());
        }

        await _mockApiHttpClient.Received(1).GetBlobAsync(expectedEndpoint, Arg.Any<CancellationToken>());
    }
}
