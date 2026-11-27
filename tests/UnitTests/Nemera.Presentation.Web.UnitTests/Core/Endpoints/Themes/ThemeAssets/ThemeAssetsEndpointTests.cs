#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Themes.ThemeAssets;
using Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Themes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Themes.ThemeAssets;

/// <summary>
/// Contains unit tests for the <see cref="ThemeAssetsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeAssetsEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly ThemeAssetsEndpoint _sut;
    private readonly GetThemeAssetRequestFixture _getThemeAssetRequestFixture = new();
    private readonly BlobDataDtoFixture _blobDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeAssetsEndpointTests"/> class.
    /// </summary>
    public ThemeAssetsEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<ThemeAssetsEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAssetPathProvided_ShouldFetchAssetWithTheResolvedPath()
    {
        // Arrange
        _mockApiHttpClient.GetBlobAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_blobDataDtoFixture.Create(data: [1, 2, 3], contentType: "text/css"));

        // Act
        IResult result = await _sut.ExecuteAsync(_getThemeAssetRequestFixture.Create(themeId: "editorial-paper", path: "assets/style.css"), CancellationToken.None);

        // Assert
        FileContentHttpResult fileResult = Assert.IsType<FileContentHttpResult>(result);
        Assert.Equal("text/css", fileResult.ContentType);
        string expectedEndpoint = ApiRoutes.Themes.GET_THEME_ASSET
            .Replace("{themeId}", "editorial-paper")
            .Replace("{*assetPath}", "assets/style.css");
        await _mockApiHttpClient.Received(1).GetBlobAsync(expectedEndpoint, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenAssetPathMissing_ShouldReturnProblem()
    {
        // Act
        IResult result = await _sut.ExecuteAsync(_getThemeAssetRequestFixture.Create(themeId: "editorial-paper", path: null), CancellationToken.None);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        await _mockApiHttpClient.DidNotReceive().GetBlobAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
