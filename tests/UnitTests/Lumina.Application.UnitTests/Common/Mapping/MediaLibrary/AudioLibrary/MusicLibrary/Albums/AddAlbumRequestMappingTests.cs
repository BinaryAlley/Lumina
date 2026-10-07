#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Contains unit tests for the <see cref="AddAlbumRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddAlbumRequestMappingTests
{
    private readonly AddAlbumRequestFixture _addAlbumRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingCompleteRequest_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        AddAlbumRequest request = _addAlbumRequestFixture.Create();

        // Act
        AddAlbumCommand result = request.ToCommand(libraryId.ToString(), artistId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryId.ToString(), result.LibraryId);
        Assert.Equal(artistId.ToString(), result.ArtistId);
        Assert.Equal(request.Metadata, result.Metadata);
        Assert.Equal(request.MediaFormat, result.MediaFormat);
        Assert.Equal(request.Barcode, result.Barcode);
        Assert.Equal(request.CatalogNumbers, result.CatalogNumbers);
        Assert.Equal(request.MusicBrainzReleaseId, result.MusicBrainzReleaseId);
        Assert.Equal(request.MusicBrainzReleaseGroupId, result.MusicBrainzReleaseGroupId);
        Assert.Equal(request.MusicBrainzReleaseArtistId, result.MusicBrainzReleaseArtistId);
        Assert.Equal(request.Contributors, result.Contributors);
        Assert.Equal(request.Ratings, result.Ratings);
        Assert.Null(result.AlbumId);
        Assert.NotNull(result.Tracks);
        Assert.Equal(request.Tracks!.Count, result.Tracks.Count);
    }

    [Fact]
    public void ToCommand_WhenMappingTracks_ShouldMapEachTrackToACommandWithTheLibraryIdAndArtistIdAndNullAlbumId()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        AddAlbumRequest request = _addAlbumRequestFixture.Create();

        // Act
        AddAlbumCommand result = request.ToCommand(libraryId.ToString(), artistId.ToString());

        // Assert
        for (int i = 0; i < request.Tracks!.Count; i++)
        {
            AddTrackCommand trackCommand = result.Tracks![i];
            Assert.Equal(libraryId.ToString(), trackCommand.LibraryId);
            Assert.Equal(artistId.ToString(), trackCommand.ArtistId);
            Assert.Null(trackCommand.AlbumId);
            Assert.Equal(request.Tracks[i].Path, trackCommand.Path);
            Assert.Equal(request.Tracks[i].Metadata, trackCommand.Metadata);
            Assert.Equal(request.Tracks[i].TrackNumber, trackCommand.TrackNumber);
        }
    }

    [Fact]
    public void ToCommand_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredPropertiesAndNulls()
    {
        // Arrange
        AddAlbumRequest request = _addAlbumRequestFixture.Create(
            includeMediaFormat: false,
            includeBarcode: false,
            includeCatalogNumbers: false,
            includeMusicBrainzReleaseId: false,
            includeMusicBrainzReleaseGroupId: false,
            includeMusicBrainzReleaseArtistId: false,
            includeTracks: false);

        // Act
        AddAlbumCommand result = request.ToCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Assert
        Assert.NotNull(result.Metadata);
        Assert.Null(result.MediaFormat);
        Assert.Null(result.Barcode);
        Assert.Null(result.CatalogNumbers);
        Assert.Null(result.MusicBrainzReleaseId);
        Assert.Null(result.MusicBrainzReleaseGroupId);
        Assert.Null(result.MusicBrainzReleaseArtistId);
        Assert.Null(result.Tracks);
    }

    [Fact]
    public void ToCommand_WhenRouteIdsAreNull_ShouldMapNullRouteIds()
    {
        // Arrange
        AddAlbumRequest request = _addAlbumRequestFixture.Create();

        // Act
        AddAlbumCommand result = request.ToCommand(null, null);

        // Assert
        Assert.Null(result.LibraryId);
        Assert.Null(result.ArtistId);
    }

    [Fact]
    public void ToCommands_WhenMappingMultipleRequests_ShouldMapEachRequest()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        List<AddAlbumRequest> requests = _addAlbumRequestFixture.CreateMany(3);

        // Act
        List<AddAlbumCommand> result = [.. requests.ToCommands(libraryId.ToString(), artistId.ToString())];

        // Assert
        Assert.Equal(requests.Count, result.Count);
        for (int i = 0; i < requests.Count; i++)
        {
            Assert.Equal(requests[i].Metadata, result[i].Metadata);
            Assert.Equal(requests[i].Tracks!.Count, result[i].Tracks!.Count);
            Assert.Equal(libraryId.ToString(), result[i].LibraryId);
            Assert.Equal(artistId.ToString(), result[i].ArtistId);
        }
    }
}
