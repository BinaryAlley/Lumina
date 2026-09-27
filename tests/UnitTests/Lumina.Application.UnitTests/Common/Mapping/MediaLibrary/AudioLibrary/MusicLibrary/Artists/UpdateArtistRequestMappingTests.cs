#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Contains unit tests for the <see cref="UpdateArtistRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistRequestMappingTests
{
    private readonly UpdateArtistRequestFixture _updateArtistRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingCompleteRequest_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        UpdateArtistRequest request = _updateArtistRequestFixture.Create();

        // Act
        UpdateArtistCommand result = request.ToCommand(libraryId.ToString(), artistId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryId.ToString(), result.LibraryId);
        Assert.Equal(artistId.ToString(), result.ArtistId);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Website, result.Website);
        Assert.Equal(request.MusicBrainzArtistId, result.MusicBrainzArtistId);
        Assert.Equal(request.Contributors, result.Contributors);
        Assert.NotNull(result.Albums);
        Assert.Equal(request.Albums!.Count, result.Albums.Count);
    }

    [Fact]
    public void ToCommand_WhenMappingAlbums_ShouldMapEachAlbumToACommandWithTheLibraryIdAndArtistId()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        UpdateArtistRequest request = _updateArtistRequestFixture.Create();

        // Act
        UpdateArtistCommand result = request.ToCommand(libraryId.ToString(), artistId.ToString());

        // Assert
        for (int i = 0; i < request.Albums!.Count; i++)
        {
            Assert.Equal(libraryId.ToString(), result.Albums![i].LibraryId);
            Assert.Equal(artistId.ToString(), result.Albums[i].ArtistId);
            Assert.Equal(request.Albums[i].AlbumId, result.Albums[i].AlbumId);
            Assert.Equal(request.Albums[i].Metadata, result.Albums[i].Metadata);
        }
    }

    [Fact]
    public void ToCommand_WhenAlbumsAreMissing_ShouldMapNullAlbums()
    {
        // Arrange
        UpdateArtistRequest request = _updateArtistRequestFixture.Create(includeAlbums: false);

        // Act
        UpdateArtistCommand result = request.ToCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.Albums);
    }

    [Fact]
    public void ToCommand_WhenRouteIdsAreNull_ShouldMapNullRouteIds()
    {
        // Arrange
        UpdateArtistRequest request = _updateArtistRequestFixture.Create();

        // Act
        UpdateArtistCommand result = request.ToCommand(null, null);

        // Assert
        Assert.Null(result.LibraryId);
        Assert.Null(result.ArtistId);
    }

    [Fact]
    public void ToCommand_WhenOptionalPropertiesAreMissing_ShouldMapNulls()
    {
        // Arrange
        UpdateArtistRequest request = _updateArtistRequestFixture.Create(
            includeWebsite: false,
            includeMusicBrainzArtistId: false,
            includeAlbums: false);

        // Act
        UpdateArtistCommand result = request.ToCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.Website);
        Assert.Null(result.MusicBrainzArtistId);
        Assert.Null(result.Albums);
    }
}
