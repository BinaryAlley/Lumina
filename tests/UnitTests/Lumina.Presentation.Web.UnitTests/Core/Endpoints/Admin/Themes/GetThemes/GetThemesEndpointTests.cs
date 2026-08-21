#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.GetThemes;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Themes.GetThemes;

/// <summary>
/// Contains unit tests for the <see cref="GetThemesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemesEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetThemesEndpoint _sut;
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();
    private readonly ThemeSettingsResponseDtoFixture _themeSettingsResponseDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemesEndpointTests"/> class.
    /// </summary>
    public GetThemesEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        ThemeService themeService = new(_mockApiHttpClient);
        _sut = Factory.Create<GetThemesEndpoint>(themeService);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsThemes_ShouldReturnSuccessJsonWithThemeAdminData()
    {
        // Arrange
        ThemeResponseDto[] themes =
        [
            _themeResponseDtoFixture.Create(themeId: "editorial-paper"),
            _themeResponseDtoFixture.Create(themeId: "lumina-default")
        ];
        ThemeResponseDto currentTheme = _themeResponseDtoFixture.Create(themeId: "lumina-default");
        _mockApiHttpClient.GetAsync<ThemeResponseDto[]>(ApiRoutes.Themes.GET_THEMES, Arg.Any<CancellationToken>())
            .Returns(themes);
        _mockApiHttpClient.GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>())
            .Returns(currentTheme);
        _mockApiHttpClient.GetAsync<ThemeSettingsResponseDto>(ApiRoutes.Themes.GET_THEME_SETTINGS, Arg.Any<CancellationToken>())
            .Returns(_themeSettingsResponseDtoFixture.Create(maxArchiveBytes: 10 * 1024 * 1024));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using (JsonDocument jsonDocument = JsonDocument.Parse(body))
        {
            Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
            JsonElement data = jsonDocument.RootElement.GetProperty("data");
            Assert.Equal(2, data.GetProperty("themes").GetArrayLength());
            Assert.Equal("editorial-paper", data.GetProperty("themes")[0].GetProperty("id").GetString());
            Assert.Equal("lumina-default", data.GetProperty("currentThemeId").GetString());
            Assert.Equal(10 * 1024 * 1024, data.GetProperty("maxArchiveBytes").GetInt64());
            Assert.Equal(10.0, data.GetProperty("maxArchiveMegabytes").GetDouble());
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldLoadThemesCurrentThemeAndSettings()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<ThemeResponseDto[]>(ApiRoutes.Themes.GET_THEMES, Arg.Any<CancellationToken>())
            .Returns([.. _themeResponseDtoFixture.CreateMany(2)]);
        _mockApiHttpClient.GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>())
            .Returns(_themeResponseDtoFixture.Create());
        _mockApiHttpClient.GetAsync<ThemeSettingsResponseDto>(ApiRoutes.Themes.GET_THEME_SETTINGS, Arg.Any<CancellationToken>())
            .Returns(_themeSettingsResponseDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ThemeResponseDto[]>(ApiRoutes.Themes.GET_THEMES, Arg.Any<CancellationToken>());
        await _mockApiHttpClient.Received(1).GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>());
        await _mockApiHttpClient.Received(1).GetAsync<ThemeSettingsResponseDto>(ApiRoutes.Themes.GET_THEME_SETTINGS, Arg.Any<CancellationToken>());
    }
}
