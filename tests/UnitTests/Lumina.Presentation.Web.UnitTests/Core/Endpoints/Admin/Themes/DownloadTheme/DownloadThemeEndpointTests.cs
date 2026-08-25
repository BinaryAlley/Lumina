#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.DownloadTheme;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Themes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Admin.Themes.DownloadTheme;

/// <summary>
/// Contains unit tests for the <see cref="DownloadThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DownloadThemeEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly DownloadThemeEndpoint _sut;
    private readonly GetThemeArchiveRequestFixture _getThemeArchiveRequestFixture = new();
    private readonly BlobDataDtoFixture _blobDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadThemeEndpointTests"/> class.
    /// </summary>
    public DownloadThemeEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        ThemeService themeService = new(_mockApiHttpClient);
        _sut = Factory.Create<DownloadThemeEndpoint>(themeService);
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeIdProvided_ShouldReturnZipFileWithResolvedFileName()
    {
        // Arrange
        string themeId = "editorial-paper";
        _mockApiHttpClient.GetBlobAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_blobDataDtoFixture.Create(data: [0x50, 0x4B, 0x03, 0x04], contentType: "application/zip"));

        // Act
        IResult result = await _sut.ExecuteAsync(_getThemeArchiveRequestFixture.Create(themeId: themeId), CancellationToken.None);

        // Assert
        FileStreamHttpResult fileResult = Assert.IsType<FileStreamHttpResult>(result);
        Assert.Equal("application/zip", fileResult.ContentType);
        Assert.Equal($"{themeId}.zip", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task ExecuteAsync_WhenThemeIdProvided_ShouldFetchArchiveWithResolvedEndpoint()
    {
        // Arrange
        string themeId = "editorial-paper";
        string expectedEndpoint = ApiRoutes.Themes.GET_THEME_ARCHIVE.Replace("{themeId}", themeId);
        _mockApiHttpClient.GetBlobAsync(expectedEndpoint, Arg.Any<CancellationToken>())
            .Returns(_blobDataDtoFixture.Create(data: [1, 2, 3], contentType: "application/zip"));

        // Act
        await _sut.ExecuteAsync(_getThemeArchiveRequestFixture.Create(themeId: themeId), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetBlobAsync(expectedEndpoint, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)] // missing theme id
    [InlineData("")] // empty theme id
    [InlineData("   ")] // whitespace theme id
    public async Task ExecuteAsync_WhenThemeIdIsBlank_ShouldReturnProblemWithBadRequest(string? themeId)
    {
        // Act
        IResult result = await _sut.ExecuteAsync(_getThemeArchiveRequestFixture.Create(themeId: themeId), CancellationToken.None);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        await _mockApiHttpClient.DidNotReceive().GetBlobAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
