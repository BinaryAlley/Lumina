#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.SaveMusicArtist;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.SaveMusicArtist;

/// <summary>
/// Contains unit tests for the <see cref="SaveMusicArtistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveMusicArtistEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly SaveMusicArtistEndpoint _sut;
    private readonly UpdateMusicArtistRequestFixture _updateMusicArtistRequestFixture = new();
    private readonly MusicArtistDtoFixture _musicArtistDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveMusicArtistEndpointTests"/> class.
    /// </summary>
    public SaveMusicArtistEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<SaveMusicArtistEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldForwardUpdateMusicArtistRequestToApiWithRouteAndQueryIds()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        UpdateMusicArtistRequest request = _updateMusicArtistRequestFixture.Create();
        MusicArtistDto expectedArtist = _musicArtistDtoFixture.Create();
        _mockApiHttpClient.PutAsync<MusicArtistDto, UpdateMusicArtistRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicArtistRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedArtist);
        ConfigureRequest(artistId.ToString(), libraryId);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<MusicArtistDto, UpdateMusicArtistRequest>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(libraryId, artistId)),
            Arg.Is<UpdateMusicArtistRequest>(forwardedRequest => forwardedRequest == request),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenArtistRouteIdIsNotParseable_ShouldForwardGuidEmptyArtistIdToApi()
    {
        // Arrange
        const string MALICIOUS_ARTIST_ID = "not-a-guid";
        Guid libraryId = Guid.NewGuid();
        UpdateMusicArtistRequest request = _updateMusicArtistRequestFixture.Create();
        _mockApiHttpClient.PutAsync<MusicArtistDto, UpdateMusicArtistRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicArtistRequest>(), Arg.Any<CancellationToken>())
            .Returns(_musicArtistDtoFixture.Create());
        ConfigureRequest(MALICIOUS_ARTIST_ID, libraryId);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<MusicArtistDto, UpdateMusicArtistRequest>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(libraryId, Guid.Empty)),
            Arg.Any<UpdateMusicArtistRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenArtistRouteIdIsMissing_ShouldForwardGuidEmptyArtistIdToApi()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        UpdateMusicArtistRequest request = _updateMusicArtistRequestFixture.Create();
        _mockApiHttpClient.PutAsync<MusicArtistDto, UpdateMusicArtistRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicArtistRequest>(), Arg.Any<CancellationToken>())
            .Returns(_musicArtistDtoFixture.Create());
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={libraryId}");

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<MusicArtistDto, UpdateMusicArtistRequest>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(libraryId, Guid.Empty)),
            Arg.Any<UpdateMusicArtistRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryQueryIdIsNotParseable_ShouldForwardGuidEmptyLibraryIdToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        const string MALICIOUS_LIBRARY_ID = "not-a-guid";
        UpdateMusicArtistRequest request = _updateMusicArtistRequestFixture.Create();
        _mockApiHttpClient.PutAsync<MusicArtistDto, UpdateMusicArtistRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicArtistRequest>(), Arg.Any<CancellationToken>())
            .Returns(_musicArtistDtoFixture.Create());
        ConfigureRequest(artistId.ToString(), MALICIOUS_LIBRARY_ID);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<MusicArtistDto, UpdateMusicArtistRequest>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(Guid.Empty, artistId)),
            Arg.Any<UpdateMusicArtistRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsUpdatedArtist_ShouldReturnSuccessJsonWithArtist()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        UpdateMusicArtistRequest request = _updateMusicArtistRequestFixture.Create();
        MusicArtistDto expectedArtist = _musicArtistDtoFixture.Create();
        _mockApiHttpClient.PutAsync<MusicArtistDto, UpdateMusicArtistRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicArtistRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedArtist);
        ConfigureRequest(artistId.ToString(), libraryId);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedArtist.Id, jsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid());
    }

    /// <summary>
    /// Sets the route and query values the endpoint reads the artist identities from.
    /// </summary>
    /// <param name="artistId">The raw route value of the artist Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    private void ConfigureRequest(string artistId, Guid libraryId)
    {
        ConfigureRequest(artistId, libraryId.ToString());
    }

    /// <summary>
    /// Sets the route and query values the endpoint reads the artist identities from.
    /// </summary>
    /// <param name="artistId">The raw route value of the artist Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    private void ConfigureRequest(string artistId, string libraryId)
    {
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId;
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={libraryId}");
    }

    /// <summary>
    /// Builds the upstream endpoint the artist update is expected to be sent to.
    /// </summary>
    /// <param name="libraryId">The normalized Id of the media library.</param>
    /// <param name="artistId">The normalized Id of the artist.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId, Guid artistId)
    {
        return ApiRoutes.Music.UPDATE_ARTIST
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString());
    }
}
