#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.DeleteConsumptionItemHistory;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.Statistics.DeleteConsumptionItemHistory;

/// <summary>
/// Contains unit tests for the <see cref="DeleteConsumptionItemHistoryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteConsumptionItemHistoryEndpointTests
{
    private static readonly Guid s_mediaItemId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid s_libraryId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly DeleteConsumptionItemHistoryEndpoint _sut;
    private readonly DeleteConsumptionItemHistoryRequestFixture _deleteConsumptionItemHistoryRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConsumptionItemHistoryEndpointTests"/> class.
    /// </summary>
    public DeleteConsumptionItemHistoryEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _mockApiHttpClient.DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _sut = Factory.Create<DeleteConsumptionItemHistoryEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithoutData()
    {
        // Arrange
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create();

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
    public async Task ExecuteAsync_WhenUserIdIsNullOrWhitespace_ShouldDeleteCurrentUserItemHistory(string? userId)
    {
        // Arrange
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create(
            userId: userId,
            includeUserId: userId is not null,
            mediaKind: MediaKind.Movie,
            mediaItemId: s_mediaItemId,
            libraryId: s_libraryId);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == "users/me/consumption-history/media-items/Movie/11111111-1111-1111-1111-111111111111/22222222-2222-2222-2222-222222222222"),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(MediaKind.MusicTrack, "MusicTrack")] // a music track
    [InlineData(MediaKind.PodcastEpisode, "PodcastEpisode")] // a podcast episode
    [InlineData(MediaKind.Book, "Book")] // a book
    public async Task ExecuteAsync_WhenMediaKindIsSet_ShouldUseTheMediaKindNameInTheUpstreamRoute(MediaKind mediaKind, string expectedMediaKind)
    {
        // Arrange
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create(
            mediaKind: mediaKind,
            mediaItemId: s_mediaItemId,
            libraryId: s_libraryId,
            includeUserId: false);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == $"users/me/consumption-history/media-items/{expectedMediaKind}/11111111-1111-1111-1111-111111111111/22222222-2222-2222-2222-222222222222"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIdIsSet_ShouldRequestUserRouteWithEscapedUserId()
    {
        // Arrange
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create(
            userId: "user/../admin?x=1&y=2",
            mediaKind: MediaKind.Book,
            mediaItemId: s_mediaItemId,
            libraryId: s_libraryId);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == "users/user%2F..%2Fadmin%3Fx%3D1%26y%3D2/consumption-history/media-items/Book/11111111-1111-1111-1111-111111111111/22222222-2222-2222-2222-222222222222"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create();

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }
}
