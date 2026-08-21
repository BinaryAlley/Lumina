#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Themes;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemesIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemesIndexViewEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly ThemesIndexViewEndpoint _sut;
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemesIndexViewEndpointTests"/> class.
    /// </summary>
    public ThemesIndexViewEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        ThemeService themeService = new(_mockApiHttpClient);
        _sut = Factory.Create<ThemesIndexViewEndpoint>(themeService);
        TestHttpContextFactory.ConfigureSession(_sut.HttpContext, TestHttpContextFactory.CreateSession());
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemesAvailable_ShouldReturnRazorViewResult()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<ThemeResponseDto[]>(ApiRoutes.Themes.GET_THEMES, Arg.Any<CancellationToken>())
            .Returns([.. _themeResponseDtoFixture.CreateMany(2)]);
        _mockApiHttpClient.GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>())
            .Returns(_themeResponseDtoFixture.Create());

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.IsType<RazorViewResult>(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldLoadThemesAndCurrentThemeFromService()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<ThemeResponseDto[]>(ApiRoutes.Themes.GET_THEMES, Arg.Any<CancellationToken>())
            .Returns([.. _themeResponseDtoFixture.CreateMany(2)]);
        _mockApiHttpClient.GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>())
            .Returns(_themeResponseDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ThemeResponseDto[]>(ApiRoutes.Themes.GET_THEMES, Arg.Any<CancellationToken>());
        await _mockApiHttpClient.Received(1).GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>());
    }
}
