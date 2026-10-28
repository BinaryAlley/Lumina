#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.ShuffleTracks;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Contains unit tests for the <see cref="ShuffleTracksRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ShuffleTracksRequestMappingTests
{
    private readonly ShuffleTracksRequestFixture _shuffleTracksRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingCompleteRequest_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid firstExcludedTrackId = Guid.NewGuid();
        Guid secondExcludedTrackId = Guid.NewGuid();
        ShuffleTracksRequest request = _shuffleTracksRequestFixture.Create(
            count: 25,
            artistId: artistId,
            albumId: albumId,
            excludedTrackIds: [firstExcludedTrackId, secondExcludedTrackId]);

        // Act
        ShuffleTracksQuery result = request.ToQuery(libraryId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryId.ToString(), result.LibraryId);
        Assert.Equal(25, result.Count);
        Assert.Equal(artistId, result.ArtistId);
        Assert.Equal(albumId, result.AlbumId);
        Assert.NotNull(result.ExcludedTrackIds);
        Assert.Equal(new[] { firstExcludedTrackId, secondExcludedTrackId }, result.ExcludedTrackIds);
    }

    [Fact]
    public void ToQuery_WhenExcludedTrackIdsAreNull_ShouldKeepThemNull()
    {
        // Arrange
        ShuffleTracksRequest request = _shuffleTracksRequestFixture.Create(includeExcludedTrackIds: false);

        // Act
        ShuffleTracksQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.ExcludedTrackIds);
    }

    [Fact]
    public void ToQuery_WhenOptionalScopeIsMissing_ShouldKeepTheScopeNull()
    {
        // Arrange
        ShuffleTracksRequest request = _shuffleTracksRequestFixture.Create(includeArtistId: false, includeAlbumId: false);

        // Act
        ShuffleTracksQuery result = request.ToQuery(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.ArtistId);
        Assert.Null(result.AlbumId);
    }

    [Fact]
    public void ToQuery_WhenLibraryIdArgumentIsNull_ShouldKeepItNull()
    {
        // Arrange
        ShuffleTracksRequest request = _shuffleTracksRequestFixture.Create();

        // Act
        ShuffleTracksQuery result = request.ToQuery(null);

        // Assert
        // The route value is forwarded untouched, so that the query validator reports the missing identifier.
        Assert.Null(result.LibraryId);
    }

    [Theory]
    [InlineData("not-a-guid")] // a value that cannot be parsed
    [InlineData("' OR '1'='1")] // an injection payload
    [InlineData("'; DROP TABLE Tracks; --")] // a destructive injection payload
    public void ToQuery_WhenLibraryIdArgumentIsNotParseable_ShouldPassItVerbatim(string libraryId)
    {
        // Arrange
        ShuffleTracksRequest request = _shuffleTracksRequestFixture.Create();

        // Act
        ShuffleTracksQuery result = request.ToQuery(libraryId);

        // Assert
        // The raw route value must reach the validator, which is the layer that decides it is not a valid identifier.
        Assert.Equal(libraryId, result.LibraryId);
        Assert.Equal(request.Count, result.Count);
    }
}
