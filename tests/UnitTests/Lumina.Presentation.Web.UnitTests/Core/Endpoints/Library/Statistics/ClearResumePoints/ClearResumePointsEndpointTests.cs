#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.ClearResumePoints;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Statistics.ClearResumePoints;

/// <summary>
/// Contains unit tests for the <see cref="ClearResumePointsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearResumePointsEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly ClearResumePointsEndpoint _sut;
    private readonly ClearResumePointsRequestFixture _clearResumePointsRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearResumePointsEndpointTests"/> class.
    /// </summary>
    public ClearResumePointsEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _mockApiHttpClient.DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _sut = Factory.Create<ClearResumePointsEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithoutData()
    {
        // Arrange
        ClearResumePointsRequest request = _clearResumePointsRequestFixture.Create();

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.False(jsonDocument.RootElement.TryGetProperty("data", out _));
    }

    [Theory]
    [InlineData(null)] // no user selected, the current user is implied
    [InlineData("")] // empty user selected
    [InlineData("   ")] // whitespace user selected
    public async Task ExecuteAsync_WhenUserIdIsNullOrWhitespaceAndLibraryIdIsNull_ShouldClearCurrentUserResumePointsWithoutFilter(string? userId)
    {
        // Arrange
        ClearResumePointsRequest request = _clearResumePointsRequestFixture.Create(
            userId: userId,
            includeUserId: userId is not null,
            includeLibraryId: false);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.LibraryStatistics.CLEAR_RESUME_POINTS),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyLibraryIdIsSet_ShouldAppendEscapedLibraryFilterToCurrentUserRoute()
    {
        // Arrange
        ClearResumePointsRequest request = _clearResumePointsRequestFixture.Create(
            libraryId: "lib?x=1&y=2",
            includeUserId: false);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == "users/me/media-resume-points?libraryId=lib%3Fx%3D1%26y%3D2"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIdAndLibraryIdAreSet_ShouldRequestUserRouteWithBothValuesEscaped()
    {
        // Arrange
        ClearResumePointsRequest request = _clearResumePointsRequestFixture.Create(
            userId: "user/../admin?x=1&y=2",
            libraryId: "lib?x=1&y=2");

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == "users/user%2F..%2Fadmin%3Fx%3D1%26y%3D2/media-resume-points?libraryId=lib%3Fx%3D1%26y%3D2"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        ClearResumePointsRequest request = _clearResumePointsRequestFixture.Create();

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }
}
