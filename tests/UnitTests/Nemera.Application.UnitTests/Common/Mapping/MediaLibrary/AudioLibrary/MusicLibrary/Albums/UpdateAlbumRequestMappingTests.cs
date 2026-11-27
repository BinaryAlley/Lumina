#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbum;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Contains unit tests for the <see cref="UpdateAlbumRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateAlbumRequestMappingTests
{
    private readonly UpdateAlbumRequestFixture _updateAlbumRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingCompleteRequest_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        UpdateAlbumRequest request = _updateAlbumRequestFixture.Create();

        // Act
        UpdateAlbumCommand result = request.ToCommand(libraryId.ToString(), artistId.ToString(), albumId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryId.ToString(), result.LibraryId);
        Assert.Equal(artistId.ToString(), result.ArtistId);
        Assert.Equal(albumId.ToString(), result.AlbumId);
        Assert.Equal(request.Metadata, result.Metadata);
        Assert.Equal(request.MediaFormat, result.MediaFormat);
        Assert.Equal(request.Barcode, result.Barcode);
        Assert.Equal(request.CatalogNumbers, result.CatalogNumbers);
        Assert.Equal(request.MusicBrainzReleaseId, result.MusicBrainzReleaseId);
        Assert.Equal(request.MusicBrainzReleaseGroupId, result.MusicBrainzReleaseGroupId);
        Assert.Equal(request.MusicBrainzReleaseArtistId, result.MusicBrainzReleaseArtistId);
        Assert.Equal(request.Contributors, result.Contributors);
        Assert.Equal(request.Ratings, result.Ratings);
    }

    [Fact]
    public void ToCommand_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredPropertiesAndNulls()
    {
        // Arrange
        UpdateAlbumRequest request = _updateAlbumRequestFixture.Create(
            includeMediaFormat: false,
            includeBarcode: false,
            includeCatalogNumbers: false,
            includeMusicBrainzReleaseId: false,
            includeMusicBrainzReleaseGroupId: false,
            includeMusicBrainzReleaseArtistId: false);

        // Act
        UpdateAlbumCommand result = request.ToCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Assert
        Assert.NotNull(result.Metadata);
        Assert.Null(result.MediaFormat);
        Assert.Null(result.Barcode);
        Assert.Null(result.CatalogNumbers);
        Assert.Null(result.MusicBrainzReleaseId);
        Assert.Null(result.MusicBrainzReleaseGroupId);
        Assert.Null(result.MusicBrainzReleaseArtistId);
    }

    [Fact]
    public void ToCommand_WhenRouteIdsAreNull_ShouldMapNullRouteIds()
    {
        // Arrange
        UpdateAlbumRequest request = _updateAlbumRequestFixture.Create();

        // Act
        UpdateAlbumCommand result = request.ToCommand(null, null, null);

        // Assert
        Assert.Null(result.LibraryId);
        Assert.Null(result.ArtistId);
        Assert.Null(result.AlbumId);
    }
}
