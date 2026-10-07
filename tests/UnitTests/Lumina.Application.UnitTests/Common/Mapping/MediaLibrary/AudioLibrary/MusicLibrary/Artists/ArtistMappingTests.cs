#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Contains unit tests for the <see cref="ArtistMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistMappingTests
{
    private readonly ArtistFixture _artistFixture = new();
    private readonly AlbumFixture _albumFixture = new();
    private readonly MusicBrainzIdFixture _musicBrainzIdFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingCompleteArtist_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Artist artist = _artistFixture.Create();

        // Act
        ArtistEntity result = artist.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(artist.Id.Value, result.Id);
        Assert.Equal(artist.LibraryId.Value, result.LibraryId);
        Assert.Equal(artist.Name, result.Name);
        Assert.Equal(artist.Website.Value, result.Website);
        Assert.Equal(artist.MusicBrainzArtistId.Value.Value, result.MusicBrainzArtistId);
        Assert.Equal(artist.CreatedOnUtc, result.CreatedOnUtc);
        Assert.Equal(Guid.Empty, result.CreatedBy);
        Assert.Null(result.UpdatedOnUtc);
        Assert.Null(result.UpdatedBy);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingContributors_ShouldCreateAContributorEntityPerContributor()
    {
        // Arrange
        Artist artist = _artistFixture.Create();

        // Act
        ArtistEntity result = artist.ToRepositoryEntity();

        // Assert
        Assert.Equal(artist.Contributors.Count, result.Contributors.Count);
        for (int i = 0; i < artist.Contributors.Count; i++)
        {
            MusicMediaContributor contributor = artist.Contributors.ElementAt(i);
            ArtistContributorEntity contributorEntity = result.Contributors[i];
            Assert.Equal(contributor.ContributorId.Value, contributorEntity.MediaContributorId);
            Assert.Equal(contributor.Role, contributorEntity.Role);
            Assert.Equal(artist.Id.Value, contributorEntity.ArtistId);
            Assert.Equal(artist.CreatedOnUtc, contributorEntity.CreatedOnUtc);
            Assert.Equal(Guid.Empty, contributorEntity.CreatedBy);
            Assert.Null(contributorEntity.UpdatedBy);
        }
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingAlbums_ShouldCreateAnAlbumEntityPerAlbum()
    {
        // Arrange
        Artist artist = _artistFixture.Create();

        // Act
        ArtistEntity result = artist.ToRepositoryEntity();

        // Assert
        Assert.Equal(artist.Albums.Count, result.Albums.Count);
        for (int i = 0; i < artist.Albums.Count; i++)
        {
            Album album = artist.Albums.ElementAt(i);
            AlbumEntity albumEntity = result.Albums[i];
            Assert.Equal(album.Id.Value, albumEntity.Id);
            Assert.Equal(artist.Id.Value, albumEntity.ArtistId);
            Assert.Equal(artist.LibraryId.Value, albumEntity.LibraryId);
            Assert.Equal(album.Metadata.Title, albumEntity.Title);
        }
    }

    [Fact]
    public void ToRepositoryEntity_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredValuesAndNulls()
    {
        // Arrange
        Album album = _albumFixture.Create(
            mediaFormat: Optional<MusicMediaFormat>.None(),
            barcode: Optional<Barcode>.None(),
            catalogNumbers: [],
            musicBrainzReleaseId: Optional<MusicBrainzId>.None(),
            musicBrainzReleaseGroupId: Optional<MusicBrainzId>.None(),
            musicBrainzReleaseArtistId: Optional<MusicBrainzId>.None());
        Artist artist = _artistFixture.Create(
            website: Optional<string>.None(),
            musicBrainzArtistId: Optional<MusicBrainzId>.None(),
            contributors: [],
            albums: [album]);

        // Act
        ArtistEntity result = artist.ToRepositoryEntity();

        // Assert
        Assert.Null(result.Website);
        Assert.Null(result.MusicBrainzArtistId);
        Assert.Empty(result.Contributors);
        Assert.Single(result.Albums);
        Assert.Null(result.Albums[0].MediaFormat);
        Assert.Null(result.Albums[0].Barcode);
        Assert.Empty(result.Albums[0].CatalogNumbers);
        Assert.Null(result.Albums[0].MusicBrainzReleaseId);
        Assert.Null(result.Albums[0].MusicBrainzReleaseGroupId);
        Assert.Null(result.Albums[0].MusicBrainzReleaseArtistId);
    }

    [Fact]
    public void ToRepositoryEntity_WhenWebsiteIsProvided_ShouldMapIt()
    {
        // Arrange
        string website = "https://www.queenonline.com";
        Artist artist = _artistFixture.Create(website: Optional<string>.Some(website));

        // Act
        ArtistEntity result = artist.ToRepositoryEntity();

        // Assert
        Assert.Equal(website, result.Website);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMusicBrainzArtistIdIsProvided_ShouldMapIt()
    {
        // Arrange
        MusicBrainzId musicBrainzArtistId = _musicBrainzIdFixture.Create();
        Artist artist = _artistFixture.Create(musicBrainzArtistId: Optional<MusicBrainzId>.Some(musicBrainzArtistId));

        // Act
        ArtistEntity result = artist.ToRepositoryEntity();

        // Assert
        Assert.Equal(musicBrainzArtistId.Value, result.MusicBrainzArtistId);
    }
}
