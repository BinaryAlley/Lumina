#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.RestoreMusicArtist;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.RestoreMusicArtist;

/// <summary>
/// Contains unit tests for the <see cref="RestoreMusicArtistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreMusicArtistEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly RestoreMusicArtistEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreMusicArtistEndpointTests"/> class.
    /// </summary>
    public RestoreMusicArtistEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<RestoreMusicArtistEndpoint>(_mockApiHttpClient);
    }

    [Theory]
    [InlineData("D")] // Standard hyphenated format.
    [InlineData("B")] // Braced format.
    [InlineData("N")] // Compact format.
    [InlineData("P")] // Parenthesized format.
    public async Task ExecuteAsync_WhenCalled_ShouldRestoreArtistViaApiWithNormalizedIdsAndReturnSuccessPayload(string guidFormat)
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest(artistId.ToString(guidFormat), libraryId.ToString(guidFormat));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PostIgnoringResponseAsync(
            BuildExpectedEndpoint(libraryId, artistId),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.True(jsonDocument.RootElement.GetProperty("data").GetProperty("isRestored").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WhenArtistRouteIdIsNotParseable_ShouldForwardGuidEmptyArtistIdToApi()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest("not-a-guid", libraryId.ToString());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostIgnoringResponseAsync(
            BuildExpectedEndpoint(libraryId, Guid.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenArtistRouteIdIsMissing_ShouldForwardGuidEmptyArtistIdToApi()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest(null, libraryId.ToString());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostIgnoringResponseAsync(
            BuildExpectedEndpoint(libraryId, Guid.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryQueryIdIsNotParseable_ShouldForwardGuidEmptyLibraryIdToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        ConfigureRequest(artistId.ToString(), "not-a-guid");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostIgnoringResponseAsync(
            BuildExpectedEndpoint(Guid.Empty, artistId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryQueryIdIsMissing_ShouldForwardGuidEmptyLibraryIdToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        ConfigureRequest(artistId.ToString(), null);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostIgnoringResponseAsync(
            BuildExpectedEndpoint(Guid.Empty, artistId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest(artistId.ToString(), libraryId.ToString());
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        _mockApiHttpClient.PostIgnoringResponseAsync(
            Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromCanceled(cancellationTokenSource.Token));

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _sut.ExecuteAsync(EmptyRequest.Instance, cancellationTokenSource.Token));
    }

    /// <summary>
    /// Sets the route and query values the endpoint reads the artist identity from.
    /// </summary>
    /// <param name="artistId">The raw route value of the artist Id, or <see langword="null"/> to leave the route value missing.</param>
    /// <param name="libraryId">The raw query value of the library Id, or <see langword="null"/> to leave the query string missing.</param>
    private void ConfigureRequest(string? artistId, string? libraryId)
    {
        if (artistId is not null)
            _sut.HttpContext.Request.RouteValues["artistId"] = artistId;
        if (libraryId is not null)
            _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={libraryId}");
    }

    /// <summary>
    /// Builds the upstream endpoint the artist restoration is expected to be sent to.
    /// </summary>
    /// <param name="libraryId">The normalized Id of the media library.</param>
    /// <param name="artistId">The normalized Id of the artist.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId, Guid artistId)
    {
        return ApiRoutes.Music.RESTORE_ARTIST
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString());
    }
}
