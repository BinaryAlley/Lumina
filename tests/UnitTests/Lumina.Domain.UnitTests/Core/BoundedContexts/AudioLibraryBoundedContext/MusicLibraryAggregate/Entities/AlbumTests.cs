#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;

/// <summary>
/// Contains unit tests for the <see cref="Album"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumTests
{
    private readonly AlbumFixture _albumFixture = new();
    private readonly TrackFixture _trackFixture = new();
    private readonly AlbumIdFixture _albumIdFixture = new();
    private readonly AlbumMetadataFixture _albumMetadataFixture = new();
    private readonly BarcodeFixture _barcodeFixture = new();
    private readonly MusicBrainzIdFixture _musicBrainzIdFixture = new();
    private readonly MusicMediaContributorFixture _musicMediaContributorFixture = new();
    private readonly AudioRatingFixture _audioRatingFixture = new();

    [Fact]
    public void AddTrack_WhenTrackIsNotInTheAlbum_ShouldAddTheTrackAndReturnSuccess()
    {
        // Arrange
        Album album = _albumFixture.Create(tracks: []);
        Track track = _trackFixture.Create();

        // Act
        Result<Created> result = album.AddTrack(track);

        // Assert
        Assert.False(result.IsFailure);
        Track storedTrack = Assert.Single(album.Tracks);
        Assert.Equal(track, storedTrack);
    }

    [Fact]
    public void AddTrack_WhenTrackIsAlreadyInTheAlbum_ShouldReturnTheTrackIsAlreadyInTheAlbumErrorAndNotAddDuplicate()
    {
        // Arrange
        Album album = _albumFixture.Create(tracks: []);
        Track track = _trackFixture.Create();
        album.AddTrack(track);

        // Act
        Result<Created> result = album.AddTrack(track);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TheTrackIsAlreadyInTheAlbum, result.FirstError);
        Assert.Single(album.Tracks);
    }

    [Fact]
    public void Create_WhenCalledWithAllProperties_ShouldCreateAlbumWithAllProperties()
    {
        // Arrange
        AlbumMetadata metadata = _albumMetadataFixture.Create(title: "A Night at the Opera");
        Optional<MusicMediaFormat> mediaFormat = Optional<MusicMediaFormat>.Some(MusicMediaFormat.Vinyl);
        Optional<Barcode> barcode = Optional<Barcode>.Some(_barcodeFixture.Create());
        Optional<string> catalogNumber = Optional<string>.Some("CAT-001");
        Optional<MusicBrainzId> releaseId = Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create());
        Optional<MusicBrainzId> releaseGroupId = Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create());
        Optional<MusicBrainzId> releaseArtistId = Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create());
        List<MusicMediaContributor> contributors = [_musicMediaContributorFixture.Create()];
        List<AudioRating> ratings = [_audioRatingFixture.Create()];
        List<Track> tracks = [_trackFixture.Create()];
        DateTime beforeCreate = DateTime.UtcNow;

        // Act
        Result<Album> result = Album.Create(
            metadata,
            mediaFormat,
            barcode,
            catalogNumber,
            releaseId,
            releaseGroupId,
            releaseArtistId,
            contributors,
            ratings,
            tracks);

        // Assert
        Assert.False(result.IsFailure);
        Album album = result.Value;
        Assert.NotEqual(Guid.Empty, album.Id.Value);
        Assert.Equal(metadata, album.Metadata);
        Assert.Equal(mediaFormat, album.MediaFormat);
        Assert.Equal(barcode, album.Barcode);
        Assert.Equal(catalogNumber, album.CatalogNumber);
        Assert.Equal(releaseId, album.MusicBrainzReleaseId);
        Assert.Equal(releaseGroupId, album.MusicBrainzReleaseGroupId);
        Assert.Equal(releaseArtistId, album.MusicBrainzReleaseArtistId);
        Assert.Equal(contributors, album.Contributors);
        Assert.Equal(ratings, album.Ratings);
        Assert.Equal(tracks, album.Tracks);
        Assert.InRange(album.CreatedOnUtc, beforeCreate, DateTime.UtcNow);
        Assert.False(album.UpdatedOnUtc.HasValue);
    }

    [Fact]
    public void Create_WhenOptionalValuesAreAbsent_ShouldCreateAlbumWithoutThem()
    {
        // Act
        Result<Album> result = Album.Create(
            _albumMetadataFixture.Create(),
            Optional<MusicMediaFormat>.None(),
            Optional<Barcode>.None(),
            Optional<string>.None(),
            Optional<MusicBrainzId>.None(),
            Optional<MusicBrainzId>.None(),
            Optional<MusicBrainzId>.None(),
            [],
            [],
            []);

        // Assert
        Assert.False(result.IsFailure);
        Album album = result.Value;
        Assert.False(album.MediaFormat.HasValue);
        Assert.False(album.Barcode.HasValue);
        Assert.False(album.CatalogNumber.HasValue);
        Assert.False(album.MusicBrainzReleaseId.HasValue);
        Assert.False(album.MusicBrainzReleaseGroupId.HasValue);
        Assert.False(album.MusicBrainzReleaseArtistId.HasValue);
    }

    [Fact]
    public void Create_WhenCollectionsAreEmpty_ShouldCreateAlbumWithEmptyCollections()
    {
        // Act
        Result<Album> result = Album.Create(
            _albumMetadataFixture.Create(),
            Optional<MusicMediaFormat>.None(),
            Optional<Barcode>.None(),
            Optional<string>.None(),
            Optional<MusicBrainzId>.None(),
            Optional<MusicBrainzId>.None(),
            Optional<MusicBrainzId>.None(),
            [],
            [],
            []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value.Contributors);
        Assert.Empty(result.Value.Ratings);
        Assert.Empty(result.Value.Tracks);
    }

    [Fact]
    public void CreateWithId_WhenCalledWithPreExistingIdAndTimestamps_ShouldPreserveIdentityAndTimestamps()
    {
        // Arrange
        AlbumId albumId = _albumIdFixture.Create();
        AlbumMetadata metadata = _albumMetadataFixture.Create();
        DateTime createdOnUtc = DateTime.UtcNow.AddDays(-1);
        DateTime updatedOnUtc = DateTime.UtcNow;

        // Act
        Result<Album> result = Album.Create(
            albumId,
            metadata,
            Optional<MusicMediaFormat>.Some(MusicMediaFormat.CD),
            Optional<Barcode>.Some(_barcodeFixture.Create()),
            Optional<string>.Some("CAT-002"),
            Optional<MusicBrainzId>.None(),
            Optional<MusicBrainzId>.None(),
            Optional<MusicBrainzId>.None(),
            [],
            [],
            [],
            createdOnUtc,
            Optional<DateTime>.Some(updatedOnUtc));

        // Assert
        Assert.False(result.IsFailure);
        Album album = result.Value;
        Assert.Equal(albumId, album.Id);
        Assert.Equal(metadata, album.Metadata);
        Assert.Equal(createdOnUtc, album.CreatedOnUtc);
        Assert.Equal(Optional<DateTime>.Some(updatedOnUtc), album.UpdatedOnUtc);
    }

    [Fact]
    public void RemoveTrack_WhenTrackIsInTheAlbum_ShouldRemoveTrackAndReturnSuccess()
    {
        // Arrange
        Track track = _trackFixture.Create();
        Album album = _albumFixture.Create(tracks: [track]);

        // Act
        Result<Deleted> result = album.RemoveTrack(track);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(album.Tracks);
        Assert.DoesNotContain(track, album.Tracks);
    }

    [Fact]
    public void RemoveTrack_WhenTrackIsNotInTheAlbum_ShouldReturnTheTrackIsNotInTheAlbumError()
    {
        // Arrange
        Track storedTrack = _trackFixture.Create();
        Track foreignTrack = _trackFixture.Create();
        Album album = _albumFixture.Create(tracks: [storedTrack]);

        // Act
        Result<Deleted> result = album.RemoveTrack(foreignTrack);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TheTrackIsNotInTheAlbum, result.FirstError);
        Assert.Single(album.Tracks);
        Assert.Equal(storedTrack, album.Tracks.Single());
    }

    [Fact]
    public void UpdateContributors_WhenCalledTwice_ShouldReplaceTheContributorsInsteadOfAppending()
    {
        // Arrange
        Album album = _albumFixture.Create(contributors: []);
        MusicMediaContributor firstContributor = _musicMediaContributorFixture.Create();
        MusicMediaContributor secondContributor = _musicMediaContributorFixture.Create();
        album.UpdateContributors([firstContributor, _musicMediaContributorFixture.Create()]);

        // Act
        album.UpdateContributors([secondContributor]);

        // Assert
        MusicMediaContributor storedContributor = Assert.Single(album.Contributors);
        Assert.Equal(secondContributor, storedContributor);
        Assert.DoesNotContain(firstContributor, album.Contributors);
    }

    [Fact]
    public void UpdateContributors_WhenCalledWithEmptyCollection_ShouldClearTheContributors()
    {
        // Arrange
        Album album = _albumFixture.Create(contributors: [_musicMediaContributorFixture.Create()]);

        // Act
        album.UpdateContributors([]);

        // Assert
        Assert.Empty(album.Contributors);
    }

    [Fact]
    public void UpdateRatings_WhenCalledTwice_ShouldReplaceTheRatingsInsteadOfAppending()
    {
        // Arrange
        Album album = _albumFixture.Create(ratings: []);
        AudioRating firstRating = _audioRatingFixture.Create(value: 1m, maxValue: 5m);
        AudioRating secondRating = _audioRatingFixture.Create(value: 4m, maxValue: 5m);
        album.UpdateRatings([firstRating, _audioRatingFixture.Create(value: 2m, maxValue: 5m)]);

        // Act
        album.UpdateRatings([secondRating]);

        // Assert
        AudioRating storedRating = Assert.Single(album.Ratings);
        Assert.Equal(secondRating, storedRating);
    }

    [Fact]
    public void UpdateRatings_WhenCalledWithEmptyCollection_ShouldClearTheRatings()
    {
        // Arrange
        Album album = _albumFixture.Create(ratings: [_audioRatingFixture.Create()]);

        // Act
        album.UpdateRatings([]);

        // Assert
        Assert.Empty(album.Ratings);
    }

    [Fact]
    public void UpdateDetails_WhenCalled_ShouldUpdateAllDetailPropertiesAndSetUpdatedOnUtc()
    {
        // Arrange
        Album album = _albumFixture.Create();
        List<Track> originalTracks = [.. album.Tracks];
        AlbumMetadata metadata = _albumMetadataFixture.Create(title: "A Day at the Races");
        Optional<MusicMediaFormat> mediaFormat = Optional<MusicMediaFormat>.Some(MusicMediaFormat.Cassette);
        Optional<Barcode> barcode = Optional<Barcode>.Some(_barcodeFixture.Create());
        Optional<string> catalogNumber = Optional<string>.Some("CAT-003");
        Optional<MusicBrainzId> releaseId = Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create());
        Optional<MusicBrainzId> releaseGroupId = Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create());
        Optional<MusicBrainzId> releaseArtistId = Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create());
        DateTime beforeUpdate = DateTime.UtcNow;

        // Act
        Result<Updated> result = album.UpdateDetails(
            metadata,
            mediaFormat,
            barcode,
            catalogNumber,
            releaseId,
            releaseGroupId,
            releaseArtistId);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(metadata, album.Metadata);
        Assert.Equal(mediaFormat, album.MediaFormat);
        Assert.Equal(barcode, album.Barcode);
        Assert.Equal(catalogNumber, album.CatalogNumber);
        Assert.Equal(releaseId, album.MusicBrainzReleaseId);
        Assert.Equal(releaseGroupId, album.MusicBrainzReleaseGroupId);
        Assert.Equal(releaseArtistId, album.MusicBrainzReleaseArtistId);
        Assert.True(album.UpdatedOnUtc.HasValue);
        Assert.True(album.UpdatedOnUtc.Value >= beforeUpdate);
        // the tracks are not part of the details update
        Assert.Equal(originalTracks, album.Tracks);
    }

    [Fact]
    public void UpdateDetails_WhenOptionalValuesAreAbsent_ShouldClearTheOptionalProperties()
    {
        // Arrange
        Album album = _albumFixture.Create();

        // Act
        Result<Updated> result = album.UpdateDetails(
            _albumMetadataFixture.Create(),
            Optional<MusicMediaFormat>.None(),
            Optional<Barcode>.None(),
            Optional<string>.None(),
            Optional<MusicBrainzId>.None(),
            Optional<MusicBrainzId>.None(),
            Optional<MusicBrainzId>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(album.MediaFormat.HasValue);
        Assert.False(album.Barcode.HasValue);
        Assert.False(album.CatalogNumber.HasValue);
        Assert.False(album.MusicBrainzReleaseId.HasValue);
        Assert.False(album.MusicBrainzReleaseGroupId.HasValue);
        Assert.False(album.MusicBrainzReleaseArtistId.HasValue);
    }
}
