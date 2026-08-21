#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Requests.Themes;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.SetCurrentTheme;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Themes.SetCurrentTheme;

/// <summary>
/// Contains unit tests for the <see cref="SetCurrentThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetCurrentThemeEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly SetCurrentThemeEndpoint _sut;
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetCurrentThemeEndpointTests"/> class.
    /// </summary>
    public SetCurrentThemeEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        ThemeService themeService = new(_mockApiHttpClient);
        _sut = Factory.Create<SetCurrentThemeEndpoint>(themeService);
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeIdProvided_ShouldSetCurrentThemeAndReturnSuccessJson()
    {
        // Arrange
        string themeId = "editorial-paper";
        _mockApiHttpClient.PutAsync<ThemeResponseDto, object>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(_themeResponseDtoFixture.Create());

        // Act
        IResult result = await _sut.ExecuteAsync(new SetCurrentThemeRequest(themeId), CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using (System.Text.Json.JsonDocument jsonDocument = System.Text.Json.JsonDocument.Parse(body))
            Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());

        await _mockApiHttpClient.Received(1).PutAsync<ThemeResponseDto, object>(
            ApiRoutes.Themes.SET_CURRENT_THEME,
            Arg.Is<object>(payload => (payload.GetType().GetProperty("ThemeId")!.GetValue(payload) as string) == themeId),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)] // missing theme id
    [InlineData("")] // empty theme id
    [InlineData("   ")] // whitespace theme id
    public async Task ExecuteAsync_WhenThemeIdIsBlank_ShouldReturnProblemWithBadRequest(string? themeId)
    {
        // Act
        IResult result = await _sut.ExecuteAsync(new SetCurrentThemeRequest(themeId), CancellationToken.None);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        await _mockApiHttpClient.DidNotReceive().PutAsync<ThemeResponseDto, object>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
