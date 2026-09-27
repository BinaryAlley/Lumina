#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.AddArtist;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.AddArtist;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Contains unit tests for the <see cref="AddArtistCommandMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddArtistCommandMappingTests
{
    private readonly AddArtistCommandFixture _addArtistCommandFixture = new();
    private readonly AddAlbumCommandFixture _addAlbumCommandFixture = new();
    private readonly AlbumMetadataDtoFixture _albumMetadataDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingCompleteCommand_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        AddArtistCommand command = _addArtistCommandFixture.Create();

        // Act
        Result<Artist> result = command.ToDomainEntity(libraryId);

        // Assert
        Assert.False(result.IsFailure);
        Artist artist = result.Value;
        Assert.Equal(libraryId, artist.LibraryId.Value);
        Assert.Equal(command.Name, artist.Name);
        Assert.True(artist.Website.HasValue);
        Assert.Equal(command.Website, artist.Website.Value);
        Assert.True(artist.MusicBrainzArtistId.HasValue);
        Assert.Equal(command.MusicBrainzArtistId, artist.MusicBrainzArtistId.Value.Value);
        Assert.Equal(command.Contributors!.Count, artist.Contributors.Count);
        Assert.Equal(command.Albums!.Count, artist.Albums.Count);
        Assert.Equal(command.Albums.Select(album => album.Metadata!.Title), artist.Albums.Select(album => album.Metadata.Title));
    }

    [Fact]
    public void ToDomainEntity_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredValuesAndNones()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        AddArtistCommand command = _addArtistCommandFixture.Create(
            includeWebsite: false,
            includeMusicBrainzArtistId: false,
            contributors: []);

        // Act
        Result<Artist> result = command.ToDomainEntity(libraryId);

        // Assert
        Assert.False(result.IsFailure);
        Artist artist = result.Value;
        Assert.False(artist.Website.HasValue);
        Assert.False(artist.MusicBrainzArtistId.HasValue);
        Assert.Empty(artist.Contributors);
        Assert.NotEmpty(artist.Albums);
    }

    [Fact]
    public void ToDomainEntity_WhenAlbumCreationFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        AddAlbumCommand invalidAlbum = _addAlbumCommandFixture.Create(
            metadata: _albumMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]));
        AddArtistCommand command = _addArtistCommandFixture.Create(albums: [invalidAlbum]);

        // Act
        Result<Artist> result = command.ToDomainEntity(libraryId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.GenreNameCannotBeEmpty.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenArtistNameIsEmpty_ShouldReturnError()
    {
        // Arrange
        AddArtistCommand command = _addArtistCommandFixture.Create(name: string.Empty);

        // Act
        Result<Artist> result = command.ToDomainEntity(Guid.NewGuid());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenArtistHasNoAlbums_ShouldReturnError()
    {
        // Arrange
        AddArtistCommand command = _addArtistCommandFixture.Create(albums: []);

        // Act
        Result<Artist> result = command.ToDomainEntity(Guid.NewGuid());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistMustHaveAtLeastOneAlbum, result.FirstError);
    }
}
