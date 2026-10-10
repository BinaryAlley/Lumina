#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;

/// <summary>
/// Contains unit tests for the <see cref="Artist"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistTests
{
    private readonly ArtistFixture _artistFixture = new();
    private readonly AlbumFixture _albumFixture = new();
    private readonly TrackFixture _trackFixture = new();
    private readonly ArtistIdFixture _artistIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly AlbumMetadataFixture _albumMetadataFixture = new();
    private readonly BarcodeFixture _barcodeFixture = new();
    private readonly MusicBrainzIdFixture _musicBrainzIdFixture = new();
    private readonly MusicMediaContributorFixture _musicMediaContributorFixture = new();
    private readonly AudioRatingFixture _audioRatingFixture = new();
    private readonly AudioMetadataFixture _audioMetadataFixture = new();
    private readonly MoodFixture _moodFixture = new();
    private readonly IsrcFixture _isrcFixture = new();
    private readonly MusicWorkFixture _musicWorkFixture = new();

    [Fact]
    public void AddTrackToAlbum_WhenAlbumIsOwnedByTheArtist_ShouldAddTheTrackAndReturnSuccess()
    {
        // Arrange
        Album album = _albumFixture.Create(tracks: []);
        Artist artist = _artistFixture.Create(albums: [album]);
        Track track = _trackFixture.Create();

        // Act
        Result<Created> result = artist.AddTrackToAlbum(album, track);

        // Assert
        Assert.False(result.IsFailure);
        Track storedTrack = Assert.Single(album.Tracks);
        Assert.Equal(track, storedTrack);
    }

    [Fact]
    public void AddTrackToAlbum_WhenAlbumIsNotOwnedByTheArtist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        Album ownedAlbum = _albumFixture.Create(tracks: []);
        Album foreignAlbum = _albumFixture.Create(tracks: []);
        Artist artist = _artistFixture.Create(albums: [ownedAlbum]);
        Track track = _trackFixture.Create();

        // Act
        Result<Created> result = artist.AddTrackToAlbum(foreignAlbum, track);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        Assert.Empty(foreignAlbum.Tracks);
    }

    [Fact]
    public void AddTrackToAlbum_WhenTrackIsAlreadyInTheAlbum_ShouldPropagateTheTrackIsAlreadyInTheAlbumError()
    {
        // Arrange
        Album album = _albumFixture.Create(tracks: []);
        Artist artist = _artistFixture.Create(albums: [album]);
        Track track = _trackFixture.Create();
        artist.AddTrackToAlbum(album, track);

        // Act
        Result<Created> result = artist.AddTrackToAlbum(album, track);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TheTrackIsAlreadyInTheAlbum, result.FirstError);
        Assert.Single(album.Tracks);
    }

    [Fact]
    public void Create_WhenCalledWithValidData_ShouldCreateArtistWithAllProperties()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        MusicBrainzId musicBrainzArtistId = _musicBrainzIdFixture.Create();
        List<MusicMediaContributor> contributors = [_musicMediaContributorFixture.Create()];
        Album album = _albumFixture.Create();
        DateTime beforeCreate = DateTime.UtcNow;

        // Act
        Result<Artist> result = Artist.Create(
            libraryId,
            "Queen",
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<MusicArtistType>.None(),
            Optional<MusicArtistGender>.None(),
            Optional<string>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<DateOnly>.None(),
            Optional<DateOnly>.None(),
            false,
            Optional<string>.Some("https://www.queenonline.com"),
            Optional<MusicBrainzId>.Some(musicBrainzArtistId),
            [],
            [],
            [],
            [],
            [],
            [],
            contributors,
            [album]);

        // Assert
        Assert.False(result.IsFailure);
        Artist artist = result.Value;
        Assert.NotEqual(Guid.Empty, artist.Id.Value);
        Assert.Equal(libraryId, artist.LibraryId);
        Assert.Equal("Queen", artist.Name);
        Assert.Equal(Optional<string>.Some("https://www.queenonline.com"), artist.Website);
        Assert.Equal(Optional<MusicBrainzId>.Some(musicBrainzArtistId), artist.MusicBrainzArtistId);
        Assert.Equal(contributors, artist.Contributors);
        Assert.Single(artist.Albums);
        Assert.Equal(album, artist.Albums.Single());
        Assert.InRange(artist.CreatedOnUtc, beforeCreate, DateTime.UtcNow);
        Assert.False(artist.UpdatedOnUtc.HasValue);
    }

    [Fact]
    public void Create_WhenOptionalValuesAreAbsent_ShouldCreateArtistWithoutThem()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        Album album = _albumFixture.Create();

        // Act
        Result<Artist> result = Artist.Create(
            libraryId,
            "Queen",
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<MusicArtistType>.None(),
            Optional<MusicArtistGender>.None(),
            Optional<string>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<DateOnly>.None(),
            Optional<DateOnly>.None(),
            false,
            Optional<string>.None(),
            Optional<MusicBrainzId>.None(),
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [album]);

        // Assert
        Assert.False(result.IsFailure);
        Artist artist = result.Value;
        Assert.False(artist.Website.HasValue);
        Assert.False(artist.MusicBrainzArtistId.HasValue);
        Assert.Empty(artist.Contributors);
        Assert.Single(artist.Albums);
    }

    [Theory]
    [InlineData(null)] // null name
    [InlineData("")] // empty name
    [InlineData("   ")] // whitespace name
    public void Create_WhenNameIsNullOrWhitespace_ShouldReturnArtistNameCannotBeEmptyError(string? name)
    {
        // Act
        Result<Artist> result = Artist.Create(
            _libraryIdFixture.Create(),
            name!,
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<MusicArtistType>.None(),
            Optional<MusicArtistGender>.None(),
            Optional<string>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<DateOnly>.None(),
            Optional<DateOnly>.None(),
            false,
            Optional<string>.None(),
            Optional<MusicBrainzId>.None(),
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [_albumFixture.Create()]);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void Create_WhenAlbumsListIsEmpty_ShouldReturnArtistMustHaveAtLeastOneAlbumError()
    {
        // Act
        Result<Artist> result = Artist.Create(
            _libraryIdFixture.Create(),
            "Queen",
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<MusicArtistType>.None(),
            Optional<MusicArtistGender>.None(),
            Optional<string>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<DateOnly>.None(),
            Optional<DateOnly>.None(),
            false,
            Optional<string>.None(),
            Optional<MusicBrainzId>.None(),
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistMustHaveAtLeastOneAlbum, result.FirstError);
    }

    [Fact]
    public void CreateWithId_WhenCalledWithValidData_ShouldPreserveIdentityAndTimestamps()
    {
        // Arrange
        ArtistId artistId = _artistIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        Album album = _albumFixture.Create();
        DateTime createdOnUtc = DateTime.UtcNow.AddDays(-1);
        DateTime updatedOnUtc = DateTime.UtcNow;

        // Act
        Result<Artist> result = Artist.Create(
            artistId,
            libraryId,
            "Queen",
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<MusicArtistType>.None(),
            Optional<MusicArtistGender>.None(),
            Optional<string>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<DateOnly>.None(),
            Optional<DateOnly>.None(),
            false,
            Optional<string>.Some("https://www.queenonline.com"),
            Optional<MusicBrainzId>.None(),
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [album],
            createdOnUtc,
            Optional<DateTime>.Some(updatedOnUtc));

        // Assert
        Assert.False(result.IsFailure);
        Artist artist = result.Value;
        Assert.Equal(artistId, artist.Id);
        Assert.Equal(libraryId, artist.LibraryId);
        Assert.Equal("Queen", artist.Name);
        Assert.Equal(Optional<string>.Some("https://www.queenonline.com"), artist.Website);
        Assert.Equal(createdOnUtc, artist.CreatedOnUtc);
        Assert.Equal(Optional<DateTime>.Some(updatedOnUtc), artist.UpdatedOnUtc);
    }

    [Theory]
    [InlineData(null)] // null name
    [InlineData("")] // empty name
    [InlineData("   ")] // whitespace name
    public void CreateWithId_WhenNameIsNullOrWhitespace_ShouldReturnArtistNameCannotBeEmptyError(string? name)
    {
        // Act
        Result<Artist> result = Artist.Create(
            _artistIdFixture.Create(),
            _libraryIdFixture.Create(),
            name!,
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<MusicArtistType>.None(),
            Optional<MusicArtistGender>.None(),
            Optional<string>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<DateOnly>.None(),
            Optional<DateOnly>.None(),
            false,
            Optional<string>.None(),
            Optional<MusicBrainzId>.None(),
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [_albumFixture.Create()],
            DateTime.UtcNow,
            Optional<DateTime>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void CreateWithId_WhenAlbumsListIsEmpty_ShouldReturnArtistMustHaveAtLeastOneAlbumError()
    {
        // Act
        Result<Artist> result = Artist.Create(
            _artistIdFixture.Create(),
            _libraryIdFixture.Create(),
            "Queen",
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<MusicArtistType>.None(),
            Optional<MusicArtistGender>.None(),
            Optional<string>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<DateOnly>.None(),
            Optional<DateOnly>.None(),
            false,
            Optional<string>.None(),
            Optional<MusicBrainzId>.None(),
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            DateTime.UtcNow,
            Optional<DateTime>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistMustHaveAtLeastOneAlbum, result.FirstError);
    }

    [Fact]
    public void AddAlbum_WhenAlbumIsNotOwnedByTheArtist_ShouldAddTheAlbumAndReturnSuccess()
    {
        // Arrange
        Album firstAlbum = _albumFixture.Create();
        Album secondAlbum = _albumFixture.Create();
        Artist artist = _artistFixture.Create(albums: [firstAlbum]);

        // Act
        Result<Created> result = artist.AddAlbum(secondAlbum);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, artist.Albums.Count);
        Assert.Contains(secondAlbum, artist.Albums);
    }

    [Fact]
    public void AddAlbum_WhenAlbumIsAlreadyOwnedByTheArtist_ShouldReturnTheArtistAlreadyHasTheAlbumError()
    {
        // Arrange
        Album album = _albumFixture.Create();
        Artist artist = _artistFixture.Create(albums: [album]);

        // Act
        Result<Created> result = artist.AddAlbum(album);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TheArtistAlreadyHasTheAlbum, result.FirstError);
        Assert.Single(artist.Albums);
    }

    [Fact]
    public void RemoveAlbum_WhenAlbumIsOwnedAndMoreThanOneAlbumExists_ShouldRemoveItAndReturnSuccess()
    {
        // Arrange
        Album firstAlbum = _albumFixture.Create();
        Album secondAlbum = _albumFixture.Create();
        Artist artist = _artistFixture.Create(albums: [firstAlbum, secondAlbum]);

        // Act
        Result<Deleted> result = artist.RemoveAlbum(firstAlbum);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(artist.Albums);
        Assert.DoesNotContain(firstAlbum, artist.Albums);
        Assert.Contains(secondAlbum, artist.Albums);
    }

    [Fact]
    public void RemoveAlbum_WhenAlbumIsNotOwnedByTheArtist_ShouldReturnTheArtistDoesNotHaveTheAlbumError()
    {
        // Arrange
        Album ownedAlbum = _albumFixture.Create();
        Album foreignAlbum = _albumFixture.Create();
        Artist artist = _artistFixture.Create(albums: [ownedAlbum]);

        // Act
        Result<Deleted> result = artist.RemoveAlbum(foreignAlbum);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TheArtistDoesNotHaveTheAlbum, result.FirstError);
        Assert.Single(artist.Albums);
    }

    [Fact]
    public void RemoveAlbum_WhenAlbumIsTheOnlyAlbumOfTheArtist_ShouldReturnArtistMustHaveAtLeastOneAlbumError()
    {
        // Arrange
        Album onlyAlbum = _albumFixture.Create();
        Artist artist = _artistFixture.Create(albums: [onlyAlbum]);

        // Act
        Result<Deleted> result = artist.RemoveAlbum(onlyAlbum);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistMustHaveAtLeastOneAlbum, result.FirstError);
        Assert.Single(artist.Albums);
    }

    [Fact]
    public void UpdateContributors_WhenCalledWithNewContributors_ShouldReplaceThemInPlace()
    {
        // Arrange
        MusicMediaContributor firstContributor = _musicMediaContributorFixture.Create();
        MusicMediaContributor secondContributor = _musicMediaContributorFixture.Create();
        Artist artist = _artistFixture.Create(contributors: [firstContributor]);

        // Act
        artist.UpdateContributors([secondContributor]);

        // Assert
        MusicMediaContributor storedContributor = Assert.Single(artist.Contributors);
        Assert.Equal(secondContributor, storedContributor);
        Assert.DoesNotContain(firstContributor, artist.Contributors);
    }

    [Fact]
    public void UpdateContributors_WhenCalledWithDuplicateContributors_ShouldKeepOnlyDistinctOnes()
    {
        // Arrange
        MusicMediaContributor contributor = _musicMediaContributorFixture.Create();
        Artist artist = _artistFixture.Create(contributors: []);

        // Act
        artist.UpdateContributors([contributor, contributor]);

        // Assert
        MusicMediaContributor storedContributor = Assert.Single(artist.Contributors);
        Assert.Equal(contributor, storedContributor);
    }

    [Fact]
    public void UpdateDetails_WhenCalledWithValidData_ShouldUpdateDetailsAndSetUpdatedOnUtc()
    {
        // Arrange
        Artist artist = _artistFixture.Create();
        MusicBrainzId musicBrainzArtistId = _musicBrainzIdFixture.Create();
        DateTime beforeUpdate = DateTime.UtcNow;

        // Act
        Result<Updated> result = artist.UpdateDetails(
            "Queen",
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<MusicArtistType>.None(),
            Optional<MusicArtistGender>.None(),
            Optional<string>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<DateOnly>.None(),
            Optional<DateOnly>.None(),
            false,
            Optional<string>.Some("https://www.queenonline.com"),
            Optional<MusicBrainzId>.Some(musicBrainzArtistId));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Queen", artist.Name);
        Assert.Equal(Optional<string>.Some("https://www.queenonline.com"), artist.Website);
        Assert.Equal(Optional<MusicBrainzId>.Some(musicBrainzArtistId), artist.MusicBrainzArtistId);
        Assert.True(artist.UpdatedOnUtc.HasValue);
        Assert.True(artist.UpdatedOnUtc.Value >= beforeUpdate);
    }

    [Fact]
    public void UpdateDetails_WhenNameIsNullOrWhitespace_ShouldReturnArtistNameCannotBeEmptyErrorAndNotMutate()
    {
        // Arrange
        Artist artist = _artistFixture.Create(name: "Queen");
        string originalName = artist.Name;
        Optional<DateTime> originalUpdatedOnUtc = artist.UpdatedOnUtc;

        // Act
        Result<Updated> result = artist.UpdateDetails(
            "   ",
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<MusicArtistType>.None(),
            Optional<MusicArtistGender>.None(),
            Optional<string>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<MusicArea>.None(),
            Optional<DateOnly>.None(),
            Optional<DateOnly>.None(),
            false,
            Optional<string>.None(),
            Optional<MusicBrainzId>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNameCannotBeEmpty, result.FirstError);
        Assert.Equal(originalName, artist.Name);
        Assert.Equal(originalUpdatedOnUtc, artist.UpdatedOnUtc);
    }

    [Fact]
    public void UpdateAlbum_WhenAlbumIsOwnedByTheArtist_ShouldUpdateItAndSetUpdatedOnUtc()
    {
        // Arrange
        Album album = _albumFixture.Create();
        Artist artist = _artistFixture.Create(albums: [album]);
        AlbumMetadata metadata = _albumMetadataFixture.Create(title: "A Night at the Opera");
        Optional<MusicMediaFormat> mediaFormat = Optional<MusicMediaFormat>.Some(MusicMediaFormat.CD);
        Optional<Barcode> barcode = Optional<Barcode>.Some(_barcodeFixture.Create());
        List<string> catalogNumbers = ["CAT-123456"];
        Optional<MusicBrainzId> releaseId = Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create());
        Optional<MusicBrainzId> releaseGroupId = Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create());
        Optional<MusicBrainzId> releaseArtistId = Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create());
        List<MusicMediaContributor> contributors = [_musicMediaContributorFixture.Create()];
        List<AudioRating> ratings = [_audioRatingFixture.Create()];
        DateTime beforeUpdate = DateTime.UtcNow;

        // Act
        Result<Updated> result = artist.UpdateAlbum(
            album,
            metadata,
            Optional<string>.None(),
            mediaFormat,
            Optional<MusicReleasePackaging>.None(),
            Optional<string>.None(),
            barcode,
            catalogNumbers,
            Optional<string>.None(),
            Optional<string>.None(),
            releaseId,
            releaseGroupId,
            releaseArtistId,
            contributors,
            ratings);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(metadata, album.Metadata);
        Assert.Equal(mediaFormat, album.MediaFormat);
        Assert.Equal(barcode, album.Barcode);
        Assert.Equal(catalogNumbers, album.CatalogNumbers);
        Assert.Equal(releaseId, album.MusicBrainzReleaseId);
        Assert.Equal(releaseGroupId, album.MusicBrainzReleaseGroupId);
        Assert.Equal(releaseArtistId, album.MusicBrainzReleaseArtistId);
        Assert.Equal(contributors, album.Contributors);
        Assert.Equal(ratings, album.Ratings);
        Assert.True(artist.UpdatedOnUtc.HasValue);
        Assert.True(artist.UpdatedOnUtc.Value >= beforeUpdate);
    }

    [Fact]
    public void UpdateAlbum_WhenAlbumIsNotOwnedByTheArtist_ShouldReturnAlbumNotFoundErrorAndNotMutate()
    {
        // Arrange
        Album ownedAlbum = _albumFixture.Create();
        Album foreignAlbum = _albumFixture.Create();
        AlbumMetadata originalMetadata = foreignAlbum.Metadata;
        Artist artist = _artistFixture.Create(albums: [ownedAlbum]);
        Optional<DateTime> originalUpdatedOnUtc = artist.UpdatedOnUtc;

        // Act
        Result<Updated> result = artist.UpdateAlbum(
            foreignAlbum,
            _albumMetadataFixture.Create(),
            Optional<string>.None(),
            Optional<MusicMediaFormat>.None(),
            Optional<MusicReleasePackaging>.None(),
            Optional<string>.None(),
            Optional<Barcode>.None(),
            [],
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<MusicBrainzId>.None(),
            Optional<MusicBrainzId>.None(),
            Optional<MusicBrainzId>.None(),
            [],
            []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        Assert.Equal(originalMetadata, foreignAlbum.Metadata);
        Assert.Equal(originalUpdatedOnUtc, artist.UpdatedOnUtc);
    }

    [Fact]
    public void UpdateTrackInAlbum_WhenAlbumAndTrackBelongToTheArtist_ShouldUpdateTheTrackAndSetUpdatedOnUtc()
    {
        // Arrange
        Track track = _trackFixture.Create();
        Album album = _albumFixture.Create(tracks: [track]);
        Artist artist = _artistFixture.Create(albums: [album]);
        string path = "C:\\Music\\queen\\bohemian-rhapsody.flac";
        AudioMetadata metadata = _audioMetadataFixture.Create();
        Optional<int> discNumber = Optional<int>.Some(1);
        Optional<string> script = Optional<string>.Some("Latin");
        Optional<MusicKey> key = Optional<MusicKey>.Some(MusicKey.BMajor);
        Optional<int> bpm = Optional<int>.Some(72);
        Optional<MusicWork> work = Optional<MusicWork>.Some(_musicWorkFixture.Create(title: "Bohemian Rhapsody"));
        Optional<MusicBrainzId> recordingId = Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create());
        Optional<MusicBrainzId> musicBrainzTrackId = Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create());
        List<Mood> moods = [_moodFixture.Create(name: "epic")];
        List<Isrc> isrcs = [_isrcFixture.Create(value: "GBUM71029604")];
        List<MusicMediaContributor> contributors = [_musicMediaContributorFixture.Create()];
        List<AudioRating> ratings = [_audioRatingFixture.Create()];
        DateTime beforeUpdate = DateTime.UtcNow;

        // Act
        Result<Updated> result = artist.UpdateTrackInAlbum(
            album,
            track,
            path,
            metadata,
            11,
            discNumber,
            script,
            key,
            bpm,
            true,
            work,
            recordingId,
            musicBrainzTrackId,
            moods,
            isrcs,
            contributors,
            ratings);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(path, track.Path);
        Assert.Equal(metadata, track.Metadata);
        Assert.Equal(11, track.TrackNumber);
        Assert.Equal(discNumber, track.DiscNumber);
        Assert.Equal(script, track.Script);
        Assert.Equal(key, track.Key);
        Assert.Equal(bpm, track.Bpm);
        Assert.True(track.IsVideo);
        Assert.Equal(work, track.Work);
        Assert.Equal(recordingId, track.MusicBrainzRecordingId);
        Assert.Equal(musicBrainzTrackId, track.MusicBrainzTrackId);
        Assert.Equal(moods, track.Moods);
        Assert.Equal(isrcs, track.Isrcs);
        Assert.Equal(contributors, track.Contributors);
        Assert.Equal(ratings, track.Ratings);
        Assert.True(artist.UpdatedOnUtc.HasValue);
        Assert.True(artist.UpdatedOnUtc.Value >= beforeUpdate);
    }

    [Fact]
    public void UpdateTrackInAlbum_WhenAlbumIsNotOwnedByTheArtist_ShouldReturnAlbumNotFoundErrorAndNotMutate()
    {
        // Arrange
        Album ownedAlbum = _albumFixture.Create();
        Track track = _trackFixture.Create();
        Album foreignAlbum = _albumFixture.Create(tracks: [track]);
        Artist artist = _artistFixture.Create(albums: [ownedAlbum]);
        string originalPath = track.Path;
        Optional<DateTime> originalUpdatedOnUtc = artist.UpdatedOnUtc;

        // Act
        Result<Updated> result = artist.UpdateTrackInAlbum(
            foreignAlbum,
            track,
            "C:\\Music\\queen\\bohemian-rhapsody.flac",
            _audioMetadataFixture.Create(),
            11,
            Optional<int>.None(),
            Optional<string>.None(),
            Optional<MusicKey>.None(),
            Optional<int>.None(),
            false,
            Optional<MusicWork>.None(),
            Optional<MusicBrainzId>.None(),
            Optional<MusicBrainzId>.None(),
            [],
            [],
            [],
            []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        Assert.Equal(originalPath, track.Path);
        Assert.Equal(originalUpdatedOnUtc, artist.UpdatedOnUtc);
    }

    [Fact]
    public void UpdateTrackInAlbum_WhenTrackIsNotInTheAlbum_ShouldReturnTrackNotFoundErrorAndNotMutate()
    {
        // Arrange
        Track track = _trackFixture.Create();
        Album album = _albumFixture.Create(tracks: []);
        Artist artist = _artistFixture.Create(albums: [album]);
        string originalPath = track.Path;
        Optional<DateTime> originalUpdatedOnUtc = artist.UpdatedOnUtc;

        // Act
        Result<Updated> result = artist.UpdateTrackInAlbum(
            album,
            track,
            "C:\\Music\\queen\\bohemian-rhapsody.flac",
            _audioMetadataFixture.Create(),
            11,
            Optional<int>.None(),
            Optional<string>.None(),
            Optional<MusicKey>.None(),
            Optional<int>.None(),
            false,
            Optional<MusicWork>.None(),
            Optional<MusicBrainzId>.None(),
            Optional<MusicBrainzId>.None(),
            [],
            [],
            [],
            []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
        Assert.Equal(originalPath, track.Path);
        Assert.Equal(originalUpdatedOnUtc, artist.UpdatedOnUtc);
    }

    [Fact]
    public void RemoveTrackFromAlbum_WhenAlbumAndTrackBelongToTheArtist_ShouldRemoveTheTrackAndReturnSuccess()
    {
        // Arrange
        Track track = _trackFixture.Create();
        Album album = _albumFixture.Create(tracks: [track]);
        Artist artist = _artistFixture.Create(albums: [album]);

        // Act
        Result<Deleted> result = artist.RemoveTrackFromAlbum(album, track);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(album.Tracks);
    }

    [Fact]
    public void RemoveTrackFromAlbum_WhenAlbumIsNotOwnedByTheArtist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        Album ownedAlbum = _albumFixture.Create();
        Track track = _trackFixture.Create();
        Album foreignAlbum = _albumFixture.Create(tracks: [track]);
        Artist artist = _artistFixture.Create(albums: [ownedAlbum]);

        // Act
        Result<Deleted> result = artist.RemoveTrackFromAlbum(foreignAlbum, track);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        Assert.Single(foreignAlbum.Tracks);
    }

    [Fact]
    public void RemoveTrackFromAlbum_WhenTrackIsNotInTheAlbum_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        Track track = _trackFixture.Create();
        Album album = _albumFixture.Create(tracks: []);
        Artist artist = _artistFixture.Create(albums: [album]);

        // Act
        Result<Deleted> result = artist.RemoveTrackFromAlbum(album, track);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
        Assert.Empty(album.Tracks);
    }
}
