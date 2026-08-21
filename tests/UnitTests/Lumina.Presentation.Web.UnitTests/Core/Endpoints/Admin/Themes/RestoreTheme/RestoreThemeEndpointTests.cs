#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Requests.Themes;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.RestoreTheme;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Themes.RestoreTheme;

/// <summary>
/// Contains unit tests for the <see cref="RestoreThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreThemeEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly RestoreThemeEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreThemeEndpointTests"/> class.
    /// </summary>
    public RestoreThemeEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        ThemeService themeService = new(_mockApiHttpClient);
        _sut = Factory.Create<RestoreThemeEndpoint>(themeService);
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeIdProvided_ShouldRestoreThemeAndReturnSuccessJson()
    {
        // Arrange
        string themeId = "editorial-paper";

        // Act
        IResult result = await _sut.ExecuteAsync(new RestoreThemeRequest(themeId), CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using (System.Text.Json.JsonDocument jsonDocument = System.Text.Json.JsonDocument.Parse(body))
        {
            Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeIdProvided_ShouldRestoreThemeWithResolvedEndpoint()
    {
        // Arrange
        string themeId = "editorial-paper";
        string expectedEndpoint = ApiRoutes.Themes.RESTORE_THEME.Replace("{themeId}", themeId);

        // Act
        await _sut.ExecuteAsync(new RestoreThemeRequest(themeId), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ThemeResponseDto, object>(expectedEndpoint, Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)] // missing theme id
    [InlineData("")] // empty theme id
    [InlineData("   ")] // whitespace theme id
    public async Task ExecuteAsync_WhenThemeIdIsBlank_ShouldReturnProblemWithBadRequest(string? themeId)
    {
        // Act
        IResult result = await _sut.ExecuteAsync(new RestoreThemeRequest(themeId), CancellationToken.None);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        await _mockApiHttpClient.DidNotReceive().PostAsync<ThemeResponseDto, object>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
