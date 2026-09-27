#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Contains unit tests for the <see cref="AlbumMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumMappingTests
{
    private readonly AlbumFixture _albumFixture = new();
    private readonly TrackFixture _trackFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingCompleteAlbum_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Album album = _albumFixture.Create();

        // Act
        AlbumEntity result = album.ToRepositoryEntity(artistId, libraryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(album.Id.Value, result.Id);
        Assert.Equal(artistId, result.ArtistId);
        Assert.Equal(libraryId, result.LibraryId);
        Assert.Equal(album.Metadata.Title, result.Title);
        Assert.Equal(album.Metadata.OriginalTitle.HasValue ? album.Metadata.OriginalTitle.Value : null, result.OriginalTitle);
        Assert.Equal(album.Metadata.Description.HasValue ? album.Metadata.Description.Value : null, result.Description);
        Assert.Equal(album.Metadata.ReleaseInfo.OriginalReleaseDate.HasValue ? album.Metadata.ReleaseInfo.OriginalReleaseDate.Value : null, result.OriginalReleaseDate);
        Assert.Equal(album.Metadata.ReleaseInfo.OriginalReleaseYear.HasValue ? album.Metadata.ReleaseInfo.OriginalReleaseYear.Value : null, result.OriginalReleaseYear);
        Assert.Equal(album.Metadata.ReleaseInfo.ReReleaseDate.HasValue ? album.Metadata.ReleaseInfo.ReReleaseDate.Value : null, result.ReReleaseDate);
        Assert.Equal(album.Metadata.ReleaseInfo.ReReleaseYear.HasValue ? album.Metadata.ReleaseInfo.ReReleaseYear.Value : null, result.ReReleaseYear);
        Assert.Equal(album.Metadata.ReleaseInfo.ReleaseCountry.HasValue ? album.Metadata.ReleaseInfo.ReleaseCountry.Value : null, result.ReleaseCountry);
        Assert.Equal(album.Metadata.ReleaseInfo.ReleaseVersion.HasValue ? album.Metadata.ReleaseInfo.ReleaseVersion.Value : null, result.ReleaseVersion);
        Assert.Equal(album.Metadata.Language.HasValue ? album.Metadata.Language.Value.LanguageCode : null, result.LanguageCode);
        Assert.Equal(album.Metadata.Language.HasValue ? album.Metadata.Language.Value.LanguageName : null, result.LanguageName);
        Assert.Equal(album.Metadata.Language.HasValue && album.Metadata.Language.Value.NativeName.HasValue ? album.Metadata.Language.Value.NativeName.Value : null, result.LanguageNativeName);
        Assert.Equal(album.Metadata.OriginalLanguage.HasValue ? album.Metadata.OriginalLanguage.Value.LanguageCode : null, result.OriginalLanguageCode);
        Assert.Equal(album.Metadata.OriginalLanguage.HasValue ? album.Metadata.OriginalLanguage.Value.LanguageName : null, result.OriginalLanguageName);
        Assert.Equal(album.Metadata.OriginalLanguage.HasValue && album.Metadata.OriginalLanguage.Value.NativeName.HasValue ? album.Metadata.OriginalLanguage.Value.NativeName.Value : null, result.OriginalLanguageNativeName);
        Assert.Equal(album.Metadata.Genres.ToRepositoryEntities().Distinct().Select(genre => genre.Name).OrderBy(name => name), result.Genres.Select(genre => genre.Name).OrderBy(name => name));
        Assert.Equal(album.Metadata.Tags.ToRepositoryEntities().Distinct().Select(tag => tag.Name).OrderBy(name => name), result.Tags.Select(tag => tag.Name).OrderBy(name => name));
        Assert.Equal(album.Metadata.ReleaseType.HasValue ? album.Metadata.ReleaseType.Value : null, result.ReleaseType);
        Assert.Equal(album.Metadata.ReleaseStatus.HasValue ? album.Metadata.ReleaseStatus.Value : null, result.ReleaseStatus);
        Assert.Equal(album.Metadata.TotalDiscs.HasValue ? album.Metadata.TotalDiscs.Value : null, result.TotalDiscs);
        Assert.Equal(album.Metadata.TotalTracks, result.TotalTracks);
        Assert.Equal(album.MediaFormat.HasValue ? album.MediaFormat.Value : null, result.MediaFormat);
        Assert.Equal(album.Barcode.HasValue ? album.Barcode.Value.Value : null, result.Barcode);
        Assert.Equal(album.CatalogNumber.HasValue ? album.CatalogNumber.Value : null, result.CatalogNumber);
        Assert.Equal(album.MusicBrainzReleaseId.HasValue ? album.MusicBrainzReleaseId.Value.Value : null, result.MusicBrainzReleaseId);
        Assert.Equal(album.MusicBrainzReleaseGroupId.HasValue ? album.MusicBrainzReleaseGroupId.Value.Value : null, result.MusicBrainzReleaseGroupId);
        Assert.Equal(album.MusicBrainzReleaseArtistId.HasValue ? album.MusicBrainzReleaseArtistId.Value.Value : null, result.MusicBrainzReleaseArtistId);
        Assert.Equal(album.Ratings.ToRepositoryEntities(), result.Ratings);
        Assert.Equal(album.CreatedOnUtc, result.CreatedOnUtc);
        Assert.Equal(Guid.Empty, result.CreatedBy);
        Assert.Null(result.UpdatedOnUtc);
        Assert.Null(result.UpdatedBy);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingContributors_ShouldCreateAContributorEntityPerContributor()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Album album = _albumFixture.Create();

        // Act
        AlbumEntity result = album.ToRepositoryEntity(artistId, libraryId);

        // Assert
        Assert.Equal(album.Contributors.Count, result.Contributors.Count);
        for (int i = 0; i < album.Contributors.Count; i++)
        {
            MusicMediaContributor contributor = album.Contributors.ElementAt(i);
            AlbumContributorEntity contributorEntity = result.Contributors[i];
            Assert.Equal(contributor.ContributorId.Value, contributorEntity.MediaContributorId);
            Assert.Equal(contributor.Role, contributorEntity.Role);
            Assert.Equal(album.Id.Value, contributorEntity.AlbumId);
            Assert.Equal(album.CreatedOnUtc, contributorEntity.CreatedOnUtc);
            Assert.Equal(Guid.Empty, contributorEntity.CreatedBy);
            Assert.Null(contributorEntity.UpdatedBy);
        }
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingTracks_ShouldCreateATrackEntityPerTrackBelongingToTheAlbumAndLibrary()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Album album = _albumFixture.Create(tracks: _trackFixture.CreateMany(2));

        // Act
        AlbumEntity result = album.ToRepositoryEntity(artistId, libraryId);

        // Assert
        Assert.Equal(2, result.Tracks.Count);
        for (int i = 0; i < album.Tracks.Count; i++)
        {
            Track track = album.Tracks.ElementAt(i);
            TrackEntity trackEntity = result.Tracks[i];
            Assert.Equal(track.Id.Value, trackEntity.Id);
            Assert.Equal(album.Id.Value, trackEntity.AlbumId);
            Assert.Equal(libraryId, trackEntity.LibraryId);
        }
    }

    [Fact]
    public void ToRepositoryEntity_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredValuesAndNulls()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Album album = _albumFixture.Create(
            mediaFormat: Optional<MusicMediaFormat>.None(),
            barcode: Optional<Barcode>.None(),
            catalogNumber: Optional<string>.None(),
            musicBrainzReleaseId: Optional<MusicBrainzId>.None(),
            musicBrainzReleaseGroupId: Optional<MusicBrainzId>.None(),
            musicBrainzReleaseArtistId: Optional<MusicBrainzId>.None(),
            contributors: [],
            ratings: [],
            tracks: []);

        // Act
        AlbumEntity result = album.ToRepositoryEntity(artistId, libraryId);

        // Assert
        Assert.Null(result.MediaFormat);
        Assert.Null(result.Barcode);
        Assert.Null(result.CatalogNumber);
        Assert.Null(result.MusicBrainzReleaseId);
        Assert.Null(result.MusicBrainzReleaseGroupId);
        Assert.Null(result.MusicBrainzReleaseArtistId);
        Assert.Empty(result.Contributors);
        Assert.Empty(result.Ratings);
        Assert.Empty(result.Tracks);
    }
}
