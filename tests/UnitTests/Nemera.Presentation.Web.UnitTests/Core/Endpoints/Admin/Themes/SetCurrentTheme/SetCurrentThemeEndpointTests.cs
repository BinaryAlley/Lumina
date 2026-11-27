#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.Requests.Themes;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Admin.Themes.SetCurrentTheme;
using Nemera.Presentation.Web.Core.Themes;
using Nemera.Presentation.Web.Fixtures.Common.Requests.Themes;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.UnitTests.Core.Endpoints.Admin.Themes.SetCurrentTheme;

/// <summary>
/// Contains unit tests for the <see cref="SetCurrentThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetCurrentThemeEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly SetCurrentThemeEndpoint _sut;
    private readonly SetCurrentThemeRequestFixture _setCurrentThemeRequestFixture = new();

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
        _mockApiHttpClient.PutIgnoringResponseAsync<SetCurrentThemeRequest>(Arg.Any<string>(), Arg.Any<SetCurrentThemeRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        IResult result = await _sut.ExecuteAsync(_setCurrentThemeRequestFixture.Create(themeId: themeId), CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using (System.Text.Json.JsonDocument jsonDocument = System.Text.Json.JsonDocument.Parse(body))
            Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());

        await _mockApiHttpClient.Received(1).PutIgnoringResponseAsync<SetCurrentThemeRequest>(
            ApiRoutes.Themes.SET_CURRENT_THEME,
            Arg.Is<SetCurrentThemeRequest>(payload => payload.ThemeId == themeId),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)] // Missing theme id.
    [InlineData("")] // Empty theme id.
    [InlineData("   ")] // Whitespace theme id.
    public async Task ExecuteAsync_WhenThemeIdIsBlank_ShouldReturnProblemWithBadRequest(string? themeId)
    {
        // Act
        IResult result = await _sut.ExecuteAsync(_setCurrentThemeRequestFixture.Create(themeId: themeId), CancellationToken.None);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        await _mockApiHttpClient.DidNotReceive().PutIgnoringResponseAsync<SetCurrentThemeRequest>(Arg.Any<string>(), Arg.Any<SetCurrentThemeRequest>(), Arg.Any<CancellationToken>());
    }
}
