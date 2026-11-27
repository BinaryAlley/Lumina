#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.DeleteMusicAlbum;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.DeleteMusicAlbum;

/// <summary>
/// Contains unit tests for the <see cref="DeleteMusicAlbumEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteMusicAlbumEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly DeleteMusicAlbumEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteMusicAlbumEndpointTests"/> class.
    /// </summary>
    public DeleteMusicAlbumEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<DeleteMusicAlbumEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldDeleteAlbumViaApiAndReturnSuccessWithoutData()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest(artistId.ToString(), albumId.ToString(), libraryId);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            BuildExpectedEndpoint(libraryId, artistId, albumId),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.False(jsonDocument.RootElement.TryGetProperty("data", out JsonElement _));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdsAreNotParseable_ShouldForwardGuidEmptyIdsToApi()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest("not-a-guid", "not-a-guid", libraryId);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            BuildExpectedEndpoint(libraryId, Guid.Empty, Guid.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdsAreMissing_ShouldForwardGuidEmptyIdsToApi()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={libraryId}");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            BuildExpectedEndpoint(libraryId, Guid.Empty, Guid.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryQueryIdIsNotParseable_ShouldForwardGuidEmptyLibraryIdToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ConfigureRequest(artistId.ToString(), albumId.ToString(), "not-a-guid");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            BuildExpectedEndpoint(Guid.Empty, artistId, albumId),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Sets the route and query values the endpoint reads the album identities from.
    /// </summary>
    /// <param name="artistId">The raw route value of the artist Id.</param>
    /// <param name="albumId">The raw route value of the album Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    private void ConfigureRequest(string artistId, string albumId, Guid libraryId)
    {
        ConfigureRequest(artistId, albumId, libraryId.ToString());
    }

    /// <summary>
    /// Sets the route and query values the endpoint reads the album identities from.
    /// </summary>
    /// <param name="artistId">The raw route value of the artist Id.</param>
    /// <param name="albumId">The raw route value of the album Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    private void ConfigureRequest(string artistId, string albumId, string libraryId)
    {
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId;
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId;
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={libraryId}");
    }

    /// <summary>
    /// Builds the upstream endpoint the album deletion is expected to be sent to.
    /// </summary>
    /// <param name="libraryId">The normalized Id of the media library.</param>
    /// <param name="artistId">The normalized Id of the artist.</param>
    /// <param name="albumId">The normalized Id of the album.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId, Guid artistId, Guid albumId)
    {
        return ApiRoutes.Music.DELETE_ALBUM
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString())
            .Replace("{albumId}", albumId.ToString());
    }
}
