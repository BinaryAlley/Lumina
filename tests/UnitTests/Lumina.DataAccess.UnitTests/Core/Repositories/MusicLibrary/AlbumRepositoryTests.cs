#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.DataAccess.Core.Repositories.MusicLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.MusicLibrary;

/// <summary>
/// Contains unit tests for the <see cref="AlbumRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly AlbumRepository _sut;
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly AlbumContributorEntityFixture _albumContributorEntityFixture = new();
    private readonly TrackContributorEntityFixture _trackContributorEntityFixture = new();
    private readonly AudioRatingEntityFixture _audioRatingEntityFixture = new();
    private readonly TrackMoodEntityFixture _trackMoodEntityFixture = new();
    private readonly TrackIsrcEntityFixture _trackIsrcEntityFixture = new();
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly GenreEntityFixture _genreEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly PaginationDataDtoFixture _paginationDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AlbumRepositoryTests"/> class.
    /// </summary>
    public AlbumRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockTrackRepository.InsertAsync(Arg.Any<TrackEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Created);
        _mockTrackRepository.UpdateAsync(Arg.Any<TrackEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Updated);
        _mockTrackRepository.GetExistingPathsAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyCollection<string>>([]));
        _sut = new AlbumRepository(_mockContext, _mockTrackRepository);
    }

    [Fact]
    public async Task InsertAsync_WhenAlbumDoesNotExist_ShouldAddAlbumToContextAndReturnCreated()
    {
        // Arrange
        AlbumEntity album = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);

        // Act
        Result<Created> result = await _sut.InsertAsync(album, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<AlbumEntity>? addedAlbum = _mockContext.ChangeTracker.Entries<AlbumEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == album.Id);
        Assert.NotNull(addedAlbum);
    }

    [Fact]
    public async Task InsertAsync_WhenAlbumWithTheSameIdAlreadyExists_ShouldReturnAlbumAlreadyExists()
    {
        // Arrange
        AlbumEntity existingAlbum = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        _mockContext.Albums.Add(existingAlbum);
        await _mockContext.SaveChangesAsync();

        AlbumEntity album = _albumEntityFixture.Create(id: existingAlbum.Id, includeTracks: false, includeMetadata: false);

        // Act
        Result<Created> result = await _sut.InsertAsync(album, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumAlreadyExists, result.FirstError);
        Assert.Single(_mockContext.ChangeTracker.Entries<AlbumEntity>()); // Only the existing album should remain tracked.
    }

    [Fact]
    public async Task InsertAsync_WhenTwoTracksOfTheSameRequestShareAPath_ShouldReturnTrackAlreadyExists()
    {
        // Arrange
        AlbumEntity album = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        album.Tracks =
        [
            _trackEntityFixture.Create(albumId: album.Id, libraryId: album.LibraryId, path: "/music/queen/duplicate.flac", includeMetadata: false),
            _trackEntityFixture.Create(albumId: album.Id, libraryId: album.LibraryId, path: "/music/queen/duplicate.flac", includeMetadata: false)
        ];

        // Act
        Result<Created> result = await _sut.InsertAsync(album, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackAlreadyExists, result.FirstError);
        // The duplicate within the request is detected before the stored paths are even consulted.
        await _mockTrackRepository.DidNotReceive().GetExistingPathsAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InsertAsync_WhenATrackPathIsAlreadyStoredInTheLibrary_ShouldReturnTrackAlreadyExists()
    {
        // Arrange
        _mockTrackRepository.GetExistingPathsAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyCollection<string>>(["/music/queen/bohemian-rhapsody.flac"]));

        AlbumEntity album = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        album.Tracks = [_trackEntityFixture.Create(albumId: album.Id, libraryId: album.LibraryId, path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false)];

        // Act
        Result<Created> result = await _sut.InsertAsync(album, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackAlreadyExists, result.FirstError);
        Assert.Empty(_mockContext.ChangeTracker.Entries<AlbumEntity>());
    }

    [Fact]
    public async Task InsertAsync_WhenAnotherLibraryHasATrackWithTheSamePath_ShouldAddAlbumToContextAndReturnCreated()
    {
        // Arrange
        AlbumEntity album = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        album.Tracks = [_trackEntityFixture.Create(albumId: album.Id, libraryId: album.LibraryId, path: "/music/queen/we-will-rock-you.flac", includeMetadata: false)];

        // Act
        Result<Created> result = await _sut.InsertAsync(album, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        // The same physical path is only unique within its library, so another library is allowed to register it.
        Assert.Contains(_mockContext.ChangeTracker.Entries<AlbumEntity>(), entry => entry.State == EntityState.Added && entry.Entity.Id == album.Id);
    }

    [Fact]
    public async Task InsertAsync_WhenExistingTagAndGenreNamesAreFound_ShouldReplaceThemWithTheStoredInstancesOnTheAlbumAndItsTracks()
    {
        // Arrange
        TagEntity existingTag = _tagEntityFixture.Create(name: "ExistingTag");
        GenreEntity existingGenre = _genreEntityFixture.Create(name: "ExistingGenre");
        _mockContext.Set<TagEntity>().Add(existingTag);
        _mockContext.Set<GenreEntity>().Add(existingGenre);
        await _mockContext.SaveChangesAsync();

        AlbumEntity album = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        album.Tags = [_tagEntityFixture.Create(name: "ExistingTag"), _tagEntityFixture.Create(name: "NewAlbumTag")];
        album.Genres = [_genreEntityFixture.Create(name: "ExistingGenre"), _genreEntityFixture.Create(name: "NewAlbumGenre")];
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: album.LibraryId, includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "ExistingTag"), _tagEntityFixture.Create(name: "NewTrackTag")];
        track.Genres = [_genreEntityFixture.Create(name: "ExistingGenre"), _genreEntityFixture.Create(name: "NewTrackGenre")];
        album.Tracks = [track];

        // Act
        Result<Created> result = await _sut.InsertAsync(album, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<AlbumEntity>? addedAlbum = _mockContext.ChangeTracker.Entries<AlbumEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == album.Id);
        Assert.NotNull(addedAlbum);
        AlbumEntity addedAlbumEntity = addedAlbum!.Entity;
        Assert.Contains(addedAlbumEntity.Tags, tag => tag.Name == "ExistingTag" && ReferenceEquals(tag, existingTag));
        Assert.Contains(addedAlbumEntity.Tags, tag => tag.Name == "NewAlbumTag" && !ReferenceEquals(tag, existingTag));
        Assert.Contains(addedAlbumEntity.Genres, genre => genre.Name == "ExistingGenre" && ReferenceEquals(genre, existingGenre));
        Assert.Contains(addedAlbumEntity.Genres, genre => genre.Name == "NewAlbumGenre" && !ReferenceEquals(genre, existingGenre));
        TrackEntity addedTrack = Assert.Single(addedAlbumEntity.Tracks);
        Assert.Contains(addedTrack.Tags, tag => tag.Name == "ExistingTag" && ReferenceEquals(tag, existingTag));
        Assert.Contains(addedTrack.Tags, tag => tag.Name == "NewTrackTag" && !ReferenceEquals(tag, existingTag));
        Assert.Contains(addedTrack.Genres, genre => genre.Name == "ExistingGenre" && ReferenceEquals(genre, existingGenre));
        Assert.Contains(addedTrack.Genres, genre => genre.Name == "NewTrackGenre" && !ReferenceEquals(genre, existingGenre));
        // The shared rows are reused instead of being duplicated.
        Assert.Single(_mockContext.Set<TagEntity>().Where(tag => tag.Name == "ExistingTag"));
        Assert.Single(_mockContext.Set<GenreEntity>().Where(genre => genre.Name == "ExistingGenre"));
    }

    [Fact]
    public async Task InsertAsync_WhenANewTagAndGenreNameIsSharedBetweenTheAlbumAndItsTracks_ShouldTrackASingleInstancePerName()
    {
        // Arrange
        AlbumEntity album = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: album.LibraryId, includeMetadata: false);
        album.Tags = [_tagEntityFixture.Create(name: "SharedNewTag")];
        album.Genres = [_genreEntityFixture.Create(name: "SharedNewGenre")];
        track.Tags = [_tagEntityFixture.Create(name: "SharedNewTag")];
        track.Genres = [_genreEntityFixture.Create(name: "SharedNewGenre")];
        album.Tracks = [track];

        // Act
        Result<Created> result = await _sut.InsertAsync(album, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        // The name shared by the album and its tracks is represented by a single tracked instance, so the same key is never tracked twice.
        EntityEntry<TagEntity> addedTag = Assert.Single(_mockContext.ChangeTracker.Entries<TagEntity>(), entry => entry.Entity.Name == "SharedNewTag");
        EntityEntry<GenreEntity> addedGenre = Assert.Single(_mockContext.ChangeTracker.Entries<GenreEntity>(), entry => entry.Entity.Name == "SharedNewGenre");
        Assert.True(ReferenceEquals(album.Tags.Single(tag => tag.Name == "SharedNewTag"), addedTag.Entity));
        Assert.True(ReferenceEquals(track.Tags.Single(tag => tag.Name == "SharedNewTag"), addedTag.Entity));
        Assert.True(ReferenceEquals(album.Genres.Single(genre => genre.Name == "SharedNewGenre"), addedGenre.Entity));
        Assert.True(ReferenceEquals(track.Genres.Single(genre => genre.Name == "SharedNewGenre"), addedGenre.Entity));
    }

    [Fact]
    public async Task InsertAsync_WhenAlbumHasNoTracksOrTagsOrGenres_ShouldStillAddAlbumAndReturnCreated()
    {
        // Arrange
        AlbumEntity album = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        album.Tags = [];
        album.Genres = [];

        // Act
        Result<Created> result = await _sut.InsertAsync(album, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<AlbumEntity>? addedAlbum = _mockContext.ChangeTracker.Entries<AlbumEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == album.Id);
        Assert.NotNull(addedAlbum);
        Assert.Empty(addedAlbum!.Entity.Tracks);
        Assert.Empty(addedAlbum.Entity.Tags);
        Assert.Empty(addedAlbum.Entity.Genres);
    }

    [Fact]
    public async Task UpdateAsync_WhenAlbumDoesNotExist_ShouldReturnAlbumNotFound()
    {
        // Arrange
        AlbumEntity data = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
    }

    [Fact]
    public async Task UpdateAsync_WhenAlbumExists_ShouldUpdateItsScalarProperties()
    {
        // Arrange
        AlbumEntity storedAlbum = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        _mockContext.Albums.Add(storedAlbum);
        await _mockContext.SaveChangesAsync();

        AlbumEntity data = _albumEntityFixture.Create(id: storedAlbum.Id, includeTracks: false, includeMetadata: false);
        data.Title = "A Night at the Opera";
        data.TotalTracks = 12;

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        AlbumEntity? retrievedAlbum = await _mockContext.Albums.FindAsync(storedAlbum.Id);
        Assert.NotNull(retrievedAlbum);
        Assert.Equal("A Night at the Opera", retrievedAlbum!.Title);
        Assert.Equal(12, retrievedAlbum.TotalTracks);
    }

    [Fact]
    public async Task UpdateAsync_WhenIncomingArtistIdAndLibraryIdDiffer_ShouldPreserveTheStoredIdentity()
    {
        // Arrange
        Guid storedArtistId = Guid.NewGuid();
        Guid storedLibraryId = Guid.NewGuid();
        AlbumEntity storedAlbum = _albumEntityFixture.Create(artistId: storedArtistId, libraryId: storedLibraryId, includeTracks: false, includeMetadata: false);
        _mockContext.Albums.Add(storedAlbum);
        await _mockContext.SaveChangesAsync();

        AlbumEntity data = _albumEntityFixture.Create(id: storedAlbum.Id, includeTracks: false, includeMetadata: false);
        data.ArtistId = Guid.NewGuid();
        data.LibraryId = Guid.NewGuid();
        data.Title = "A Night at the Opera";

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        AlbumEntity? retrievedAlbum = await _mockContext.Albums.FindAsync(storedAlbum.Id);
        Assert.NotNull(retrievedAlbum);
        Assert.Equal(storedArtistId, retrievedAlbum!.ArtistId);
        Assert.Equal(storedLibraryId, retrievedAlbum.LibraryId);
    }

    [Fact]
    public async Task UpdateAsync_WhenIncomingAuditColumnsDiffer_ShouldPreserveTheStoredAuditColumns()
    {
        // Arrange
        DateTime storedCreatedOnUtc = new(2020, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        Guid storedCreatedBy = Guid.NewGuid();
        AlbumEntity storedAlbum = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        storedAlbum.CreatedOnUtc = storedCreatedOnUtc;
        storedAlbum.CreatedBy = storedCreatedBy;
        storedAlbum.UpdatedOnUtc = null;
        storedAlbum.UpdatedBy = null;
        _mockContext.Albums.Add(storedAlbum);
        await _mockContext.SaveChangesAsync();

        AlbumEntity data = _albumEntityFixture.Create(id: storedAlbum.Id, includeTracks: false, includeMetadata: false);
        data.Title = "A Night at the Opera";
        data.CreatedOnUtc = DateTime.UtcNow.AddYears(-10);
        data.CreatedBy = Guid.NewGuid();
        data.UpdatedOnUtc = DateTime.UtcNow;
        data.UpdatedBy = Guid.NewGuid();

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        AlbumEntity? retrievedAlbum = await _mockContext.Albums.FindAsync(storedAlbum.Id);
        Assert.NotNull(retrievedAlbum);
        Assert.Equal("A Night at the Opera", retrievedAlbum!.Title);
        // The audit columns are owned by the auditing interceptor alone, so an edit never overwrites them.
        Assert.Equal(storedCreatedOnUtc, retrievedAlbum.CreatedOnUtc);
        Assert.Equal(storedCreatedBy, retrievedAlbum.CreatedBy);
        Assert.Null(retrievedAlbum.UpdatedOnUtc);
        Assert.Null(retrievedAlbum.UpdatedBy);
    }

    [Fact]
    public async Task UpdateAsync_WhenRatingsChange_ShouldReplaceTheChangedAddTheNewAndRemoveTheMissingOnes()
    {
        // Arrange
        AlbumEntity storedAlbum = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        storedAlbum.Ratings =
        [
            _audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100),
            _audioRatingEntityFixture.Create(value: 5, maxValue: 5, source: AudioRatingSource.User, voteCount: 10),
            _audioRatingEntityFixture.Create(value: 2, maxValue: 5, source: AudioRatingSource.LastFm, voteCount: 20)
        ];
        _mockContext.Albums.Add(storedAlbum);
        await _mockContext.SaveChangesAsync();

        AlbumEntity data = _albumEntityFixture.Create(id: storedAlbum.Id, includeTracks: false, includeMetadata: false);
        data.Ratings =
        [
            _audioRatingEntityFixture.Create(value: 3, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100),
            _audioRatingEntityFixture.Create(value: 5, maxValue: 5, source: AudioRatingSource.User, voteCount: 10),
            _audioRatingEntityFixture.Create(value: 1, maxValue: 5, source: AudioRatingSource.Discogs, voteCount: 5)
        ];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        AlbumEntity? retrievedAlbum = await _mockContext.Albums.Include(album => album.Ratings).FirstOrDefaultAsync(album => album.Id == storedAlbum.Id);
        Assert.NotNull(retrievedAlbum);
        List<AudioRatingEntity> ratings = retrievedAlbum!.Ratings;
        Assert.Equal(3, ratings.Count);
        // The rating whose value changed is replaced, the unchanged one is kept, the missing one is removed and the new one is added.
        Assert.Contains(ratings, rating => rating.Source == AudioRatingSource.MusicBrainz && rating.Value == 3M);
        Assert.Contains(ratings, rating => rating.Source == AudioRatingSource.User && rating.Value == 5M);
        Assert.Contains(ratings, rating => rating.Source == AudioRatingSource.Discogs && rating.Value == 1M);
        Assert.DoesNotContain(ratings, rating => rating.Source == AudioRatingSource.LastFm);
    }

    [Fact]
    public async Task UpdateAsync_WhenAnUnchangedContributorIsSubmitted_ShouldPreserveItsStoredIdentityAndAuditColumns()
    {
        // Arrange
        Guid keptMediaContributorId = Guid.NewGuid();
        AlbumEntity storedAlbum = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        AlbumContributorEntity keptContributor = _albumContributorEntityFixture.Create(
            albumId: storedAlbum.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Producer);
        storedAlbum.Contributors = [keptContributor];
        _mockContext.Albums.Add(storedAlbum);
        await _mockContext.SaveChangesAsync();

        Guid keptContributorId = keptContributor.Id;
        DateTime keptContributorCreatedOnUtc = keptContributor.CreatedOnUtc;

        AlbumEntity data = _albumEntityFixture.Create(id: storedAlbum.Id, includeTracks: false, includeMetadata: false);
        data.Contributors =
            [_albumContributorEntityFixture.Create(albumId: storedAlbum.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Producer)];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        AlbumContributorEntity storedContributor = Assert.Single(
            await _mockContext.Set<AlbumContributorEntity>().Where(contributor => contributor.AlbumId == storedAlbum.Id).ToListAsync());
        Assert.Equal(keptContributorId, storedContributor.Id);
        Assert.Equal(keptContributorCreatedOnUtc, storedContributor.CreatedOnUtc);
    }

    [Fact]
    public async Task UpdateAsync_WhenContributorsChange_ShouldDeleteTheRemovedAndInsertTheAddedOnes()
    {
        // Arrange
        Guid keptMediaContributorId = Guid.NewGuid();
        Guid removedMediaContributorId = Guid.NewGuid();
        Guid addedMediaContributorId = Guid.NewGuid();
        AlbumEntity storedAlbum = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        AlbumContributorEntity keptContributor = _albumContributorEntityFixture.Create(
            albumId: storedAlbum.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Producer);
        AlbumContributorEntity removedContributor = _albumContributorEntityFixture.Create(
            albumId: storedAlbum.Id, mediaContributorId: removedMediaContributorId, role: MediaContributorRole.Composer);
        storedAlbum.Contributors = [keptContributor, removedContributor];
        _mockContext.Albums.Add(storedAlbum);
        await _mockContext.SaveChangesAsync();

        Guid keptContributorId = keptContributor.Id;

        AlbumEntity data = _albumEntityFixture.Create(id: storedAlbum.Id, includeTracks: false, includeMetadata: false);
        data.Contributors =
        [
            _albumContributorEntityFixture.Create(albumId: storedAlbum.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Producer),
            _albumContributorEntityFixture.Create(albumId: storedAlbum.Id, mediaContributorId: addedMediaContributorId, role: MediaContributorRole.Vocals)
        ];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        List<AlbumContributorEntity> storedContributors = await _mockContext.Set<AlbumContributorEntity>()
            .Where(contributor => contributor.AlbumId == storedAlbum.Id)
            .ToListAsync();
        Assert.Equal(2, storedContributors.Count);
        // The unchanged participation keeps its identity, while only the delta is deleted and inserted.
        Assert.Equal(keptContributorId, storedContributors.Single(contributor => contributor.MediaContributorId == keptMediaContributorId).Id);
        Assert.DoesNotContain(storedContributors, contributor => contributor.MediaContributorId == removedMediaContributorId);
        Assert.Contains(storedContributors, contributor => contributor.MediaContributorId == addedMediaContributorId);
    }

    [Fact]
    public async Task UpdateAsync_WhenTagsAndGenresAlreadyExistByName_ShouldReuseTheStoredRows()
    {
        // Arrange
        TagEntity existingTag = _tagEntityFixture.Create(name: "ExistingTag");
        GenreEntity existingGenre = _genreEntityFixture.Create(name: "ExistingGenre");
        AlbumEntity storedAlbum = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        storedAlbum.Tags = [existingTag];
        storedAlbum.Genres = [existingGenre];
        _mockContext.Albums.Add(storedAlbum);
        await _mockContext.SaveChangesAsync();

        AlbumEntity data = _albumEntityFixture.Create(id: storedAlbum.Id, includeTracks: false, includeMetadata: false);
        data.Tags = [_tagEntityFixture.Create(name: "ExistingTag")];
        data.Genres = [_genreEntityFixture.Create(name: "ExistingGenre")];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        AlbumEntity? retrievedAlbum = await _mockContext.Albums
            .Include(album => album.Tags)
            .Include(album => album.Genres)
            .FirstOrDefaultAsync(album => album.Id == storedAlbum.Id);
        Assert.NotNull(retrievedAlbum);
        TagEntity retrievedTag = Assert.Single(retrievedAlbum!.Tags);
        GenreEntity retrievedGenre = Assert.Single(retrievedAlbum.Genres);
        Assert.True(ReferenceEquals(retrievedTag, existingTag));
        Assert.True(ReferenceEquals(retrievedGenre, existingGenre));
        // The shared rows are reused instead of being duplicated.
        Assert.Single(_mockContext.Set<TagEntity>().Where(tag => tag.Name == "ExistingTag"));
        Assert.Single(_mockContext.Set<GenreEntity>().Where(genre => genre.Name == "ExistingGenre"));
    }

    [Fact]
    public async Task GetByIdAsync_WhenAlbumExists_ShouldReturnTheAlbum()
    {
        // Arrange
        AlbumEntity album = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        _mockContext.Albums.Add(album);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<AlbumEntity?> result = await _sut.GetByIdAsync(album.Id, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(album.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAlbumDoesNotExist_ShouldReturnNull()
    {
        // Act
        Result<AlbumEntity?> result = await _sut.GetByIdAsync(Guid.NewGuid(), cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNavigationPropertiesAreIncluded_ShouldPopulateTheRelatedCollections()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        AlbumEntity album = CreateAlbumWithFullAggregate(artist.Id, libraryId);
        _mockContext.Artists.Add(artist);
        _mockContext.Albums.Add(album);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<AlbumEntity?> result = await _sut.GetByIdAsync(album.Id, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        AlbumEntity retrievedAlbum = result.Value!;
        Assert.NotNull(retrievedAlbum.Artist);
        Assert.Equal(artist.Id, retrievedAlbum.Artist!.Id);
        Assert.Single(retrievedAlbum.Ratings);
        Assert.Equal(2, retrievedAlbum.Tags.Count);
        Assert.Equal(2, retrievedAlbum.Genres.Count);
        Assert.Single(retrievedAlbum.Contributors);
        TrackEntity retrievedTrack = Assert.Single(retrievedAlbum.Tracks);
        Assert.Single(retrievedTrack.Ratings);
        Assert.Equal(2, retrievedTrack.Tags.Count);
        Assert.Equal(2, retrievedTrack.Genres.Count);
        Assert.Equal(2, retrievedTrack.Moods.Count);
        Assert.Equal(2, retrievedTrack.Isrcs.Count);
        Assert.Single(retrievedTrack.Contributors);
    }

    [Fact]
    public async Task GetByArtistIdAsync_WhenCalled_ShouldReturnOnlyTheArtistAlbumsOrderedByTitleThenId()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        AlbumEntity betaAlbum = _albumEntityFixture.Create(artistId: artistId, title: "Beta", includeTracks: false, includeMetadata: false);
        AlbumEntity secondAlphaAlbum = _albumEntityFixture.Create(artistId: artistId, title: "Alpha", includeTracks: false, includeMetadata: false);
        AlbumEntity firstAlphaAlbum = _albumEntityFixture.Create(artistId: artistId, title: "Alpha", includeTracks: false, includeMetadata: false);
        AlbumEntity albumOfAnotherArtist = _albumEntityFixture.Create(title: "Gamma", includeTracks: false, includeMetadata: false);
        _mockContext.Albums.AddRange(betaAlbum, secondAlphaAlbum, firstAlphaAlbum, albumOfAnotherArtist);
        await _mockContext.SaveChangesAsync();

        List<AlbumEntity> artistAlbums = [betaAlbum, secondAlphaAlbum, firstAlphaAlbum];
        Guid[] expectedOrder = [.. artistAlbums.OrderBy(album => album.Title).ThenBy(album => album.Id).Select(album => album.Id)];

        // Act
        Result<IReadOnlyList<AlbumEntity>> result = await _sut.GetByArtistIdAsync(artistId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedOrder, result.Value.Select(album => album.Id));
        Assert.DoesNotContain(result.Value, album => album.Id == albumOfAnotherArtist.Id);
    }

    [Fact]
    public async Task GetByArtistIdAsync_WhenCalled_ShouldIncludeTheRelatedCollections()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        AlbumEntity album = CreateAlbumWithFullAggregate(artist.Id, libraryId);
        _mockContext.Artists.Add(artist);
        _mockContext.Albums.Add(album);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<AlbumEntity>> result = await _sut.GetByArtistIdAsync(artist.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        AlbumEntity retrievedAlbum = Assert.Single(result.Value);
        Assert.Single(retrievedAlbum.Ratings);
        Assert.Equal(2, retrievedAlbum.Tags.Count);
        Assert.Equal(2, retrievedAlbum.Genres.Count);
        Assert.Single(retrievedAlbum.Contributors);
        TrackEntity retrievedTrack = Assert.Single(retrievedAlbum.Tracks);
        Assert.Single(retrievedTrack.Ratings);
        Assert.Equal(2, retrievedTrack.Tags.Count);
        Assert.Equal(2, retrievedTrack.Moods.Count);
        Assert.Single(retrievedTrack.Contributors);
    }

    [Fact]
    public async Task GetAlbumsLiteByArtistIdAsync_WhenPaginationDataIsNull_ShouldReturnAllArtistAlbumsOrdered()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        AlbumEntity secondAlbum = _albumEntityFixture.Create(artistId: artistId, title: "Album B", includeTracks: false, includeMetadata: false);
        AlbumEntity firstAlbum = _albumEntityFixture.Create(artistId: artistId, title: "Album A", includeTracks: false, includeMetadata: false);
        AlbumEntity albumOfAnotherArtist = _albumEntityFixture.Create(title: "Album C", includeTracks: false, includeMetadata: false);
        _mockContext.Albums.AddRange(secondAlbum, firstAlbum, albumOfAnotherArtist);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<AlbumLiteRow>> result = await _sut.GetAlbumsLiteByArtistIdAsync(artistId, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        PaginatedResultDto<AlbumLiteRow> page = result.Value;
        Assert.Equal(2, page.Count);
        Assert.Equal(2, page.Data.Count);
        Assert.Equal(1, page.CurrentPage);
        Assert.Equal(2, page.PerPage);
        Assert.Equal(1, page.NumberOfPages);
        Assert.Equal([firstAlbum.Id, secondAlbum.Id], page.Data.Select(row => row.Id));
    }

    [Theory]
    [InlineData(1, 2, 1, 2)] // The first page is fully returned.
    [InlineData(2, 2, 2, 2)] // A middle page is fully returned.
    [InlineData(3, 2, 3, 1)] // The last page is partially returned.
    [InlineData(99, 2, 3, 1)] // A page beyond the last one is clamped to the last page.
    public async Task GetAlbumsLiteByArtistIdAsync_WhenPaginationIsProvided_ShouldReturnTheRequestedPage(int requestedPage, int perPage, int expectedCurrentPage, int expectedRowCount)
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        List<AlbumEntity> albums = _albumEntityFixture.CreateMany(5);
        for (int index = 0; index < albums.Count; index++)
        {
            albums[index].ArtistId = artistId;
            albums[index].Title = $"Album {index}";
            albums[index].Tracks = [];
        }
        _mockContext.Albums.AddRange(albums);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: requestedPage, perPage: perPage);

        // Act
        Result<PaginatedResultDto<AlbumLiteRow>> result = await _sut.GetAlbumsLiteByArtistIdAsync(artistId, paginationData, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        PaginatedResultDto<AlbumLiteRow> page = result.Value;
        Assert.Equal(5, page.Count);
        Assert.Equal(3, page.NumberOfPages);
        Assert.Equal(expectedCurrentPage, page.CurrentPage);
        Assert.Equal(perPage, page.PerPage);
        Assert.Equal(expectedRowCount, page.Data.Count);
    }

    [Fact]
    public async Task GetAlbumsLiteByArtistIdAsync_WhenCalled_ShouldReturnOnlyTheArtistAlbums()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        AlbumEntity albumOfArtist = _albumEntityFixture.Create(artistId: artistId, includeTracks: false, includeMetadata: false);
        AlbumEntity albumOfAnotherArtist = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        _mockContext.Albums.AddRange(albumOfArtist, albumOfAnotherArtist);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);

        // Act
        Result<PaginatedResultDto<AlbumLiteRow>> result = await _sut.GetAlbumsLiteByArtistIdAsync(artistId, paginationData, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        AlbumLiteRow row = Assert.Single(result.Value.Data);
        Assert.Equal(albumOfArtist.Id, row.Id);
        Assert.Equal(1, result.Value.Count);
    }

    [Fact]
    public async Task GetAlbumsLiteByArtistIdAsync_WhenCalled_ShouldProjectTheLiteFields()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(artistId: artistId, title: "A Night at the Opera", includeTracks: false, includeMetadata: false);
        album.TotalTracks = 12;
        _mockContext.Albums.Add(album);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);

        // Act
        Result<PaginatedResultDto<AlbumLiteRow>> result = await _sut.GetAlbumsLiteByArtistIdAsync(artistId, paginationData, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        AlbumLiteRow row = Assert.Single(result.Value.Data);
        Assert.Equal(album.Id, row.Id);
        Assert.Equal("A Night at the Opera", row.Title);
        Assert.Equal(12, row.TotalTracks);
    }

    /// <summary>
    /// Creates an album that belongs to the artist identified by <paramref name="artistId"/>, with every related collection populated deterministically.
    /// </summary>
    /// <param name="artistId">The Id of the artist that owns the album.</param>
    /// <param name="libraryId">The Id of the library that owns the album.</param>
    /// <returns>The created album.</returns>
    private AlbumEntity CreateAlbumWithFullAggregate(Guid artistId, Guid libraryId)
    {
        AlbumEntity album = _albumEntityFixture.Create(artistId: artistId, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100)];
        album.Tags = [_tagEntityFixture.Create(name: "AlbumTag1"), _tagEntityFixture.Create(name: "AlbumTag2")];
        album.Genres = [_genreEntityFixture.Create(name: "AlbumGenre1"), _genreEntityFixture.Create(name: "AlbumGenre2")];
        album.Contributors = [_albumContributorEntityFixture.Create(albumId: album.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Producer)];
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        track.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.User, voteCount: 10)];
        track.Tags = [_tagEntityFixture.Create(name: "TrackTag1"), _tagEntityFixture.Create(name: "TrackTag2")];
        track.Genres = [_genreEntityFixture.Create(name: "TrackGenre1"), _genreEntityFixture.Create(name: "TrackGenre2")];
        track.Moods = [_trackMoodEntityFixture.Create(name: "Calm"), _trackMoodEntityFixture.Create(name: "Upbeat")];
        track.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678"), _trackIsrcEntityFixture.Create(value: "GBAYE0000001")];
        track.Contributors = [_trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)];
        album.Tracks = [track];
        return album;
    }
}
