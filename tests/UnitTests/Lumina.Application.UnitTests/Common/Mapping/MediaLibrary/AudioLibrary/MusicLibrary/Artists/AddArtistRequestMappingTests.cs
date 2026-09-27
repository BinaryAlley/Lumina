#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.AddArtist;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Contains unit tests for the <see cref="AddArtistRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddArtistRequestMappingTests
{
    private readonly AddArtistRequestFixture _addArtistRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingCompleteRequest_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        AddArtistRequest request = _addArtistRequestFixture.Create();

        // Act
        AddArtistCommand result = request.ToCommand(libraryId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryId.ToString(), result.LibraryId);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Website, result.Website);
        Assert.Equal(request.MusicBrainzArtistId, result.MusicBrainzArtistId);
        Assert.Equal(request.Contributors, result.Contributors);
        Assert.NotNull(result.Albums);
        Assert.Equal(request.Albums!.Count, result.Albums.Count);
    }

    [Fact]
    public void ToCommand_WhenMappingAlbums_ShouldMapEachAlbumToACommandWithTheLibraryIdAndNullArtistId()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        AddArtistRequest request = _addArtistRequestFixture.Create();

        // Act
        AddArtistCommand result = request.ToCommand(libraryId.ToString());

        // Assert
        for (int i = 0; i < request.Albums!.Count; i++)
        {
            Assert.Equal(libraryId.ToString(), result.Albums![i].LibraryId);
            Assert.Null(result.Albums[i].ArtistId);
            Assert.Equal(request.Albums[i].Metadata, result.Albums[i].Metadata);
            Assert.Equal(request.Albums[i].MediaFormat, result.Albums[i].MediaFormat);
            Assert.Equal(request.Albums[i].Barcode, result.Albums[i].Barcode);
            Assert.Equal(request.Albums[i].CatalogNumber, result.Albums[i].CatalogNumber);
            Assert.Equal(request.Albums[i].MusicBrainzReleaseId, result.Albums[i].MusicBrainzReleaseId);
            Assert.Equal(request.Albums[i].MusicBrainzReleaseGroupId, result.Albums[i].MusicBrainzReleaseGroupId);
            Assert.Equal(request.Albums[i].MusicBrainzReleaseArtistId, result.Albums[i].MusicBrainzReleaseArtistId);
            Assert.Equal(request.Albums[i].Contributors, result.Albums[i].Contributors);
            Assert.Equal(request.Albums[i].Ratings, result.Albums[i].Ratings);
            Assert.Null(result.Albums[i].AlbumId);
        }
    }

    [Fact]
    public void ToCommand_WhenAlbumsAreMissing_ShouldMapNullAlbums()
    {
        // Arrange
        AddArtistRequest request = _addArtistRequestFixture.Create(includeAlbums: false);

        // Act
        AddArtistCommand result = request.ToCommand(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.Albums);
    }

    [Fact]
    public void ToCommand_WhenRouteIdIsNull_ShouldMapNullRouteId()
    {
        // Arrange
        AddArtistRequest request = _addArtistRequestFixture.Create();

        // Act
        AddArtistCommand result = request.ToCommand(null);

        // Assert
        Assert.Null(result.LibraryId);
    }

    [Fact]
    public void ToCommand_WhenOptionalPropertiesAreMissing_ShouldMapNulls()
    {
        // Arrange
        AddArtistRequest request = _addArtistRequestFixture.Create(
            includeWebsite: false,
            includeMusicBrainzArtistId: false,
            includeAlbums: false);

        // Act
        AddArtistCommand result = request.ToCommand(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.Website);
        Assert.Null(result.MusicBrainzArtistId);
        Assert.Null(result.Albums);
    }
}
