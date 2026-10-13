#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetMusicArtist;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetMusicArtist;

/// <summary>
/// Contains unit tests for the <see cref="GetMusicArtistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicArtistEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetMusicArtistEndpoint _sut;
    private readonly GetMusicArtistViewRequestFixture _getMusicArtistViewRequestFixture = new();
    private readonly MusicArtistDtoFixture _musicArtistDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicArtistEndpointTests"/> class.
    /// </summary>
    public GetMusicArtistEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetMusicArtistEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestArtistFromApiWithNormalizedIds()
    {
        // Arrange
        GetMusicArtistViewRequest request = _getMusicArtistViewRequestFixture.Create();
        MusicArtistDto expectedArtist = _musicArtistDtoFixture.Create();
        _mockApiHttpClient.GetAsync<MusicArtistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedArtist);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<MusicArtistDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(request.LibraryId, Guid.Parse(request.ArtistId))),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteArtistIdIsNotParseable_ShouldForwardGuidEmptyArtistIdToApi()
    {
        // Arrange
        GetMusicArtistViewRequest request = _getMusicArtistViewRequestFixture.Create(artistId: "not-a-guid");
        _mockApiHttpClient.GetAsync<MusicArtistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_musicArtistDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<MusicArtistDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(request.LibraryId, Guid.Empty)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteArtistIdIsMissing_ShouldForwardGuidEmptyArtistIdToApi()
    {
        // Arrange
        GetMusicArtistViewRequest request = _getMusicArtistViewRequestFixture.Create(artistId: string.Empty);
        _mockApiHttpClient.GetAsync<MusicArtistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_musicArtistDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<MusicArtistDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(request.LibraryId, Guid.Empty)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsArtist_ShouldReturnSuccessJsonWithArtist()
    {
        // Arrange
        GetMusicArtistViewRequest request = _getMusicArtistViewRequestFixture.Create();
        MusicArtistDto expectedArtist = _musicArtistDtoFixture.Create();
        _mockApiHttpClient.GetAsync<MusicArtistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedArtist);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedArtist.Id, jsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldForwardCancellationTokenToApi()
    {
        // Arrange
        GetMusicArtistViewRequest request = _getMusicArtistViewRequestFixture.Create();
        using CancellationTokenSource cancellationTokenSource = new();
        _mockApiHttpClient.GetAsync<MusicArtistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_musicArtistDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, cancellationTokenSource.Token);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<MusicArtistDto>(
            Arg.Any<string>(),
            Arg.Is(cancellationTokenSource.Token));
    }

    /// <summary>
    /// Builds the upstream endpoint the artist retrieval is expected to be sent to.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <param name="artistId">The normalized Id of the artist.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId, Guid artistId)
    {
        return ApiRoutes.Music.GET_ARTIST
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString());
    }
}
