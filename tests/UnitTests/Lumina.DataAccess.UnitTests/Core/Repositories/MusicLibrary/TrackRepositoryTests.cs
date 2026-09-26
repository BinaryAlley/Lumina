#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
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
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.MusicLibrary;

/// <summary>
/// Contains unit tests for the <see cref="TrackRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly TrackRepository _sut;
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly TrackMoodEntityFixture _trackMoodEntityFixture = new();
    private readonly TrackIsrcEntityFixture _trackIsrcEntityFixture = new();
    private readonly TrackContributorEntityFixture _trackContributorEntityFixture = new();
    private readonly AudioRatingEntityFixture _audioRatingEntityFixture = new();
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly GenreEntityFixture _genreEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly PaginationDataDtoFixture _paginationDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackRepositoryTests"/> class.
    /// </summary>
    public TrackRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new TrackRepository(_mockContext);
    }

    [Fact]
    public async Task InsertAsync_WhenTrackDoesNotExist_ShouldAddTrackToContextAndReturnCreated()
    {
        // Arrange
        TrackEntity track = _trackEntityFixture.Create(includeMetadata: false);

        // Act
        Result<Created> result = await _sut.InsertAsync(track, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<TrackEntity>? addedTrack = _mockContext.ChangeTracker.Entries<TrackEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == track.Id);
        Assert.NotNull(addedTrack);
    }

    [Fact]
    public async Task InsertAsync_WhenTrackWithTheSameIdAlreadyExists_ShouldReturnTrackAlreadyExists()
    {
        // Arrange
        TrackEntity existingTrack = _trackEntityFixture.Create(includeMetadata: false);
        _mockContext.Tracks.Add(existingTrack);
        await _mockContext.SaveChangesAsync();

        TrackEntity track = _trackEntityFixture.Create(id: existingTrack.Id, includeMetadata: false);

        // Act
        Result<Created> result = await _sut.InsertAsync(track, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackAlreadyExists, result.FirstError);
        Assert.Single(_mockContext.ChangeTracker.Entries<TrackEntity>()); // Only the existing track should remain tracked.
    }

    [Fact]
    public async Task InsertAsync_WhenAnotherTrackOfTheSameLibraryHasTheSamePath_ShouldReturnTrackAlreadyExists()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        TrackEntity existingTrack = _trackEntityFixture.Create(libraryId: libraryId, path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false);
        _mockContext.Tracks.Add(existingTrack);
        await _mockContext.SaveChangesAsync();

        TrackEntity track = _trackEntityFixture.Create(libraryId: libraryId, path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false);

        // Act
        Result<Created> result = await _sut.InsertAsync(track, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task InsertAsync_WhenAnotherLibraryHasATrackWithTheSamePath_ShouldAddTrackToContextAndReturnCreated()
    {
        // Arrange
        TrackEntity existingTrack = _trackEntityFixture.Create(path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false);
        _mockContext.Tracks.Add(existingTrack);
        await _mockContext.SaveChangesAsync();

        TrackEntity track = _trackEntityFixture.Create(libraryId: Guid.NewGuid(), path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false);

        // Act
        Result<Created> result = await _sut.InsertAsync(track, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        // The same physical path is only unique within its library, so the second library is allowed to register it.
        Assert.Single(_mockContext.ChangeTracker.Entries<TrackEntity>(), entry => entry.State == EntityState.Added);
    }

    [Fact]
    public async Task InsertAsync_WhenExistingTagsAreFoundByName_ShouldReplaceThemWithTheStoredInstances()
    {
        // Arrange
        TagEntity existingTag = _tagEntityFixture.Create(name: "Existing");
        _mockContext.Set<TagEntity>().Add(existingTag);
        await _mockContext.SaveChangesAsync();

        TrackEntity track = _trackEntityFixture.Create(includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "Existing"), _tagEntityFixture.Create(name: "New")];

        // Act
        Result<Created> result = await _sut.InsertAsync(track, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<TrackEntity>? addedTrack = _mockContext.ChangeTracker.Entries<TrackEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == track.Id);
        Assert.NotNull(addedTrack);
        TrackEntity addedTrackEntity = addedTrack!.Entity;
        Assert.Equal(2, addedTrackEntity.Tags.Count);
        Assert.Contains(addedTrackEntity.Tags, tag => tag.Name == "Existing" && ReferenceEquals(tag, existingTag));
        Assert.Contains(addedTrackEntity.Tags, tag => tag.Name == "New" && !ReferenceEquals(tag, existingTag));
    }

    [Fact]
    public async Task InsertAsync_WhenExistingGenresAreFoundByName_ShouldReplaceThemWithTheStoredInstances()
    {
        // Arrange
        GenreEntity existingGenre = _genreEntityFixture.Create(name: "Existing");
        _mockContext.Set<GenreEntity>().Add(existingGenre);
        await _mockContext.SaveChangesAsync();

        TrackEntity track = _trackEntityFixture.Create(includeMetadata: false);
        track.Genres = [_genreEntityFixture.Create(name: "Existing"), _genreEntityFixture.Create(name: "New")];

        // Act
        Result<Created> result = await _sut.InsertAsync(track, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<TrackEntity>? addedTrack = _mockContext.ChangeTracker.Entries<TrackEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == track.Id);
        Assert.NotNull(addedTrack);
        TrackEntity addedTrackEntity = addedTrack!.Entity;
        Assert.Equal(2, addedTrackEntity.Genres.Count);
        Assert.Contains(addedTrackEntity.Genres, genre => genre.Name == "Existing" && ReferenceEquals(genre, existingGenre));
        Assert.Contains(addedTrackEntity.Genres, genre => genre.Name == "New" && !ReferenceEquals(genre, existingGenre));
    }

    [Fact]
    public async Task InsertAsync_WhenTrackHasNoTagsOrGenres_ShouldStillAddTrackAndReturnCreated()
    {
        // Arrange
        TrackEntity track = _trackEntityFixture.Create(includeMetadata: false);
        track.Tags = [];
        track.Genres = [];

        // Act
        Result<Created> result = await _sut.InsertAsync(track, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<TrackEntity>? addedTrack = _mockContext.ChangeTracker.Entries<TrackEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == track.Id);
        Assert.NotNull(addedTrack);
        Assert.Empty(addedTrack!.Entity.Tags);
        Assert.Empty(addedTrack.Entity.Genres);
    }

    [Fact]
    public async Task GetExistingPathsAsync_WhenPathsIsEmpty_ShouldReturnAnEmptyCollection()
    {
        // Arrange
        IReadOnlyCollection<string> paths = [];

        // Act
        Result<IReadOnlyCollection<string>> result = await _sut.GetExistingPathsAsync(Guid.NewGuid(), paths, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetExistingPathsAsync_WhenCalled_ShouldReturnOnlyThePathsUsedByTracksOfTheLibrary()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        TrackEntity trackOfLibrary = _trackEntityFixture.Create(libraryId: libraryId, path: "/music/queen/love-of-my-life.flac", includeMetadata: false);
        TrackEntity trackOfAnotherLibrary = _trackEntityFixture.Create(libraryId: Guid.NewGuid(), path: "/music/queen/we-will-rock-you.flac", includeMetadata: false);
        _mockContext.Tracks.AddRange(trackOfLibrary, trackOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyCollection<string>> result = await _sut.GetExistingPathsAsync(libraryId, ["/music/queen/love-of-my-life.flac", "/music/queen/we-will-rock-you.flac"], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        string existingPath = Assert.Single(result.Value);
        Assert.Equal("/music/queen/love-of-my-life.flac", existingPath);
    }

    [Fact]
    public async Task GetExistingPathsAsync_WhenInputContainsDuplicatePaths_ShouldDeDuplicateTheResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(libraryId: libraryId, path: "/music/queen/love-of-my-life.flac", includeMetadata: false);
        _mockContext.Tracks.Add(track);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyCollection<string>> result = await _sut.GetExistingPathsAsync(libraryId, ["/music/queen/love-of-my-life.flac", "/music/queen/love-of-my-life.flac", "/music/queen/love-of-my-life.flac"], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        Assert.Equal("/music/queen/love-of-my-life.flac", result.Value.First());
    }

    [Fact]
    public async Task GetExistingPathsAsync_WhenPathDiffersOnlyByCase_ShouldNotReportItAsExisting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(libraryId: libraryId, path: "/Music/Queen/Love-Of-My-Life.flac", includeMetadata: false);
        _mockContext.Tracks.Add(track);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyCollection<string>> result = await _sut.GetExistingPathsAsync(libraryId, ["/music/queen/love-of-my-life.flac"], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task UpdateAsync_WhenTrackExists_ShouldUpdateItsScalarProperties()
    {
        // Arrange
        TrackEntity storedTrack = _trackEntityFixture.Create(includeMetadata: false);
        _mockContext.Tracks.Add(storedTrack);
        await _mockContext.SaveChangesAsync();

        TrackEntity data = _trackEntityFixture.Create(id: storedTrack.Id, includeMetadata: false);
        data.Title = "Somebody to Love";
        data.TrackNumber = 7;

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        TrackEntity? retrievedTrack = await _mockContext.Tracks.FindAsync(storedTrack.Id);
        Assert.NotNull(retrievedTrack);
        Assert.Equal("Somebody to Love", retrievedTrack!.Title);
        Assert.Equal(7, retrievedTrack.TrackNumber);
    }

    [Fact]
    public async Task UpdateAsync_WhenTrackDoesNotExist_ShouldReturnTrackNotFound()
    {
        // Arrange
        TrackEntity data = _trackEntityFixture.Create(includeMetadata: false);

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
    }

    [Fact]
    public async Task UpdateAsync_WhenIncomingIdentityColumnsDiffer_ShouldPreserveTheStoredIdentity()
    {
        // Arrange
        Guid storedAlbumId = Guid.NewGuid();
        Guid storedLibraryId = Guid.NewGuid();
        TrackEntity storedTrack = _trackEntityFixture.Create(albumId: storedAlbumId, libraryId: storedLibraryId, includeMetadata: false);
        _mockContext.Tracks.Add(storedTrack);
        await _mockContext.SaveChangesAsync();

        TrackEntity data = _trackEntityFixture.Create(id: storedTrack.Id, includeMetadata: false);
        data.AlbumId = Guid.NewGuid();
        data.LibraryId = Guid.NewGuid();

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity? retrievedTrack = await _mockContext.Tracks.FindAsync(storedTrack.Id);
        Assert.NotNull(retrievedTrack);
        Assert.Equal(storedAlbumId, retrievedTrack!.AlbumId);
        Assert.Equal(storedLibraryId, retrievedTrack.LibraryId);
    }

    [Fact]
    public async Task UpdateAsync_WhenIncomingAuditColumnsDiffer_ShouldPreserveTheStoredAuditColumns()
    {
        // Arrange
        DateTime storedCreatedOnUtc = new(2020, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        Guid storedCreatedBy = Guid.NewGuid();
        TrackEntity storedTrack = _trackEntityFixture.Create(includeMetadata: false);
        storedTrack.CreatedOnUtc = storedCreatedOnUtc;
        storedTrack.CreatedBy = storedCreatedBy;
        storedTrack.UpdatedOnUtc = null;
        storedTrack.UpdatedBy = null;
        _mockContext.Tracks.Add(storedTrack);
        await _mockContext.SaveChangesAsync();

        TrackEntity data = _trackEntityFixture.Create(id: storedTrack.Id, includeMetadata: false);
        data.Title = "Don't Stop Me Now";
        data.CreatedOnUtc = DateTime.UtcNow.AddYears(-10);
        data.CreatedBy = Guid.NewGuid();
        data.UpdatedOnUtc = DateTime.UtcNow;
        data.UpdatedBy = Guid.NewGuid();

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity? retrievedTrack = await _mockContext.Tracks.FindAsync(storedTrack.Id);
        Assert.NotNull(retrievedTrack);
        Assert.Equal("Don't Stop Me Now", retrievedTrack!.Title);
        // The audit columns are owned by the auditing interceptor alone, so an edit never overwrites them.
        Assert.Equal(storedCreatedOnUtc, retrievedTrack.CreatedOnUtc);
        Assert.Equal(storedCreatedBy, retrievedTrack.CreatedBy);
        Assert.Null(retrievedTrack.UpdatedOnUtc);
        Assert.Null(retrievedTrack.UpdatedBy);
    }

    [Fact]
    public async Task UpdateAsync_WhenAnUnchangedContributorIsSubmitted_ShouldPreserveItsStoredIdentityAndAuditColumns()
    {
        // Arrange
        Guid keptMediaContributorId = Guid.NewGuid();
        TrackEntity storedTrack = _trackEntityFixture.Create(includeMetadata: false);
        TrackContributorEntity keptContributor = _trackContributorEntityFixture.Create(
            trackId: storedTrack.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Composer);
        storedTrack.Contributors = [keptContributor];
        _mockContext.Tracks.Add(storedTrack);
        await _mockContext.SaveChangesAsync();

        Guid keptContributorId = keptContributor.Id;
        DateTime keptContributorCreatedOnUtc = keptContributor.CreatedOnUtc;

        TrackEntity data = _trackEntityFixture.Create(id: storedTrack.Id, includeMetadata: false);
        data.Contributors =
            [_trackContributorEntityFixture.Create(trackId: storedTrack.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Composer)];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        TrackContributorEntity storedContributor = Assert.Single(
            await _mockContext.Set<TrackContributorEntity>().Where(contributor => contributor.TrackId == storedTrack.Id).ToListAsync());
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
        TrackEntity storedTrack = _trackEntityFixture.Create(includeMetadata: false);
        TrackContributorEntity keptContributor = _trackContributorEntityFixture.Create(
            trackId: storedTrack.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Composer);
        TrackContributorEntity removedContributor = _trackContributorEntityFixture.Create(
            trackId: storedTrack.Id, mediaContributorId: removedMediaContributorId, role: MediaContributorRole.Producer);
        storedTrack.Contributors = [keptContributor, removedContributor];
        _mockContext.Tracks.Add(storedTrack);
        await _mockContext.SaveChangesAsync();

        Guid keptContributorId = keptContributor.Id;

        TrackEntity data = _trackEntityFixture.Create(id: storedTrack.Id, includeMetadata: false);
        data.Contributors =
        [
            _trackContributorEntityFixture.Create(trackId: storedTrack.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Composer),
            _trackContributorEntityFixture.Create(trackId: storedTrack.Id, mediaContributorId: addedMediaContributorId, role: MediaContributorRole.Vocals)
        ];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        List<TrackContributorEntity> storedContributors = await _mockContext.Set<TrackContributorEntity>()
            .Where(contributor => contributor.TrackId == storedTrack.Id)
            .ToListAsync();
        Assert.Equal(2, storedContributors.Count);
        // The unchanged participation keeps its identity, while only the delta is deleted and inserted.
        Assert.Equal(keptContributorId, storedContributors.Single(contributor => contributor.MediaContributorId == keptMediaContributorId).Id);
        Assert.DoesNotContain(storedContributors, contributor => contributor.MediaContributorId == removedMediaContributorId);
        Assert.Contains(storedContributors, contributor => contributor.MediaContributorId == addedMediaContributorId);
    }

    [Fact]
    public async Task UpdateAsync_WhenRatingsChange_ShouldReplaceTheChangedAddTheNewAndRemoveTheMissingOnes()
    {
        // Arrange
        TrackEntity storedTrack = _trackEntityFixture.Create(includeMetadata: false);
        storedTrack.Ratings =
        [
            _audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100),
            _audioRatingEntityFixture.Create(value: 5, maxValue: 5, source: AudioRatingSource.User, voteCount: 10),
            _audioRatingEntityFixture.Create(value: 2, maxValue: 5, source: AudioRatingSource.LastFm, voteCount: 20)
        ];
        _mockContext.Tracks.Add(storedTrack);
        await _mockContext.SaveChangesAsync();

        TrackEntity data = _trackEntityFixture.Create(id: storedTrack.Id, includeMetadata: false);
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
        TrackEntity? retrievedTrack = await _mockContext.Tracks.Include(track => track.Ratings).FirstOrDefaultAsync(track => track.Id == storedTrack.Id);
        Assert.NotNull(retrievedTrack);
        List<AudioRatingEntity> ratings = retrievedTrack!.Ratings;
        Assert.Equal(3, ratings.Count);
        // The rating whose value changed is replaced, the unchanged one is kept, the missing one is removed and the new one is added.
        Assert.Contains(ratings, rating => rating.Source == AudioRatingSource.MusicBrainz && rating.Value == 3M);
        Assert.Contains(ratings, rating => rating.Source == AudioRatingSource.User && rating.Value == 5M);
        Assert.Contains(ratings, rating => rating.Source == AudioRatingSource.Discogs && rating.Value == 1M);
        Assert.DoesNotContain(ratings, rating => rating.Source == AudioRatingSource.LastFm);
    }

    [Fact]
    public async Task UpdateAsync_WhenTagsAndGenresAlreadyExistByName_ShouldReuseTheStoredRows()
    {
        // Arrange
        TagEntity existingTag = _tagEntityFixture.Create(name: "ExistingTag");
        GenreEntity existingGenre = _genreEntityFixture.Create(name: "ExistingGenre");
        TrackEntity storedTrack = _trackEntityFixture.Create(includeMetadata: false);
        storedTrack.Tags = [existingTag];
        storedTrack.Genres = [existingGenre];
        _mockContext.Tracks.Add(storedTrack);
        await _mockContext.SaveChangesAsync();

        TrackEntity data = _trackEntityFixture.Create(id: storedTrack.Id, includeMetadata: false);
        data.Tags = [_tagEntityFixture.Create(name: "ExistingTag")];
        data.Genres = [_genreEntityFixture.Create(name: "ExistingGenre")];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity? retrievedTrack = await _mockContext.Tracks
            .Include(track => track.Tags)
            .Include(track => track.Genres)
            .FirstOrDefaultAsync(track => track.Id == storedTrack.Id);
        Assert.NotNull(retrievedTrack);
        TagEntity retrievedTag = Assert.Single(retrievedTrack!.Tags);
        GenreEntity retrievedGenre = Assert.Single(retrievedTrack.Genres);
        Assert.True(ReferenceEquals(retrievedTag, existingTag));
        Assert.True(ReferenceEquals(retrievedGenre, existingGenre));
        // The shared rows are reused instead of being duplicated.
        Assert.Single(_mockContext.Set<TagEntity>().Where(tag => tag.Name == "ExistingTag"));
        Assert.Single(_mockContext.Set<GenreEntity>().Where(genre => genre.Name == "ExistingGenre"));
    }

    [Fact]
    public async Task UpdateAsync_WhenMoodsAndIsrcsAreUnchanged_ShouldNotDuplicateThem()
    {
        // Arrange
        TrackEntity storedTrack = _trackEntityFixture.Create(includeMetadata: false);
        storedTrack.Moods = [_trackMoodEntityFixture.Create(name: "Calm"), _trackMoodEntityFixture.Create(name: "Energetic")];
        storedTrack.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678"), _trackIsrcEntityFixture.Create(value: "GBAYE0000001")];
        _mockContext.Tracks.Add(storedTrack);
        await _mockContext.SaveChangesAsync();

        TrackEntity data = _trackEntityFixture.Create(id: storedTrack.Id, includeMetadata: false);
        data.Moods = [_trackMoodEntityFixture.Create(name: "Calm"), _trackMoodEntityFixture.Create(name: "Upbeat")];
        data.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678"), _trackIsrcEntityFixture.Create(value: "GBAYE0000002")];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity? retrievedTrack = await _mockContext.Tracks
            .Include(track => track.Moods)
            .Include(track => track.Isrcs)
            .FirstOrDefaultAsync(track => track.Id == storedTrack.Id);
        Assert.NotNull(retrievedTrack);
        Assert.Equal(2, retrievedTrack!.Moods.Count);
        Assert.Single(retrievedTrack.Moods, mood => mood.Name == "Calm");
        Assert.Contains(retrievedTrack.Moods, mood => mood.Name == "Upbeat");
        Assert.Equal(2, retrievedTrack.Isrcs.Count);
        Assert.Single(retrievedTrack.Isrcs, isrc => isrc.Value == "USRC12345678");
        Assert.Contains(retrievedTrack.Isrcs, isrc => isrc.Value == "GBAYE0000002");
    }

    [Fact]
    public async Task GetByIdAsync_WhenTrackExists_ShouldReturnTheTrack()
    {
        // Arrange
        TrackEntity track = _trackEntityFixture.Create(includeMetadata: false);
        _mockContext.Tracks.Add(track);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<TrackEntity?> result = await _sut.GetByIdAsync(track.Id, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(track.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTrackDoesNotExist_ShouldReturnNull()
    {
        // Act
        Result<TrackEntity?> result = await _sut.GetByIdAsync(Guid.NewGuid(), cancellationToken: CancellationToken.None);

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
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "Tag1"), _tagEntityFixture.Create(name: "Tag2")];
        track.Genres = [_genreEntityFixture.Create(name: "Genre1"), _genreEntityFixture.Create(name: "Genre2")];
        track.Moods = [_trackMoodEntityFixture.Create(name: "Calm"), _trackMoodEntityFixture.Create(name: "Upbeat")];
        track.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678"), _trackIsrcEntityFixture.Create(value: "GBAYE0000001")];
        track.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100)];
        track.Contributors = [_trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)];
        _mockContext.Artists.Add(artist);
        _mockContext.Albums.Add(album);
        _mockContext.Tracks.Add(track);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<TrackEntity?> result = await _sut.GetByIdAsync(track.Id, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity retrievedTrack = result.Value!;
        Assert.NotNull(retrievedTrack.Album);
        Assert.NotNull(retrievedTrack.Album!.Artist);
        Assert.Equal(2, retrievedTrack.Tags.Count);
        Assert.Equal(2, retrievedTrack.Genres.Count);
        Assert.Equal(2, retrievedTrack.Moods.Count);
        Assert.Equal(2, retrievedTrack.Isrcs.Count);
        Assert.Single(retrievedTrack.Ratings);
        Assert.Single(retrievedTrack.Contributors);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntitiesShouldNotBeTracked_ShouldReturnAnUntrackedTrack()
    {
        // Arrange
        // The EntityFrameworkCore.Testing mocked context does not honour AsNoTracking, because its mocked DbSet returns the already tracked instances,
        // so a real in-memory provider context is used for this contract, which is a query provider feature rather than repository logic.
        // The navigation properties are not requested here, because the include path uses AsSplitQuery, which the in-memory provider does not support;
        // the include path is covered by the mocked context tests and by the integration tests.
        DbContextOptions<LuminaDbContext> options = new DbContextOptionsBuilder<LuminaDbContext>()
            .UseInMemoryDatabase($"TrackRepositoryTests-NoTracking-{Guid.NewGuid()}")
            .Options;
        using (LuminaDbContext context = new(options))
        {
            TrackRepository sut = new(context);
            TrackEntity track = _trackEntityFixture.Create(includeMetadata: false);
            context.Tracks.Add(track);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Act
            Result<TrackEntity?> result = await sut.GetByIdAsync(
                track.Id, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, CancellationToken.None);

            // Assert
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.Empty(context.ChangeTracker.Entries<TrackEntity>());
        }
    }

    [Fact]
    public async Task GetByAlbumIdAsync_WhenCalled_ShouldReturnOnlyTheAlbumTracksOrderedByDiscThenTrackThenId()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        TrackEntity secondDiscTrack = _trackEntityFixture.Create(albumId: albumId, path: "/music/queen/dont-stop-me-now.flac", trackNumber: 1, includeMetadata: false);
        secondDiscTrack.DiscNumber = 2;
        TrackEntity firstDiscSecondTrack = _trackEntityFixture.Create(albumId: albumId, path: "/music/queen/somebody-to-love.flac", trackNumber: 2, includeMetadata: false);
        firstDiscSecondTrack.DiscNumber = 1;
        TrackEntity firstDiscFirstTrack = _trackEntityFixture.Create(albumId: albumId, path: "/music/queen/killer-queen.flac", trackNumber: 1, includeMetadata: false);
        firstDiscFirstTrack.DiscNumber = 1;
        TrackEntity firstDiscFirstTieTrack = _trackEntityFixture.Create(albumId: albumId, path: "/music/queen/seven-seas-of-rhye.flac", trackNumber: 1, includeMetadata: false);
        firstDiscFirstTieTrack.DiscNumber = 1;
        TrackEntity trackOfAnotherAlbum = _trackEntityFixture.Create(path: "/music/queen/under-pressure.flac", includeMetadata: false);
        _mockContext.Tracks.AddRange(secondDiscTrack, firstDiscSecondTrack, firstDiscFirstTrack, firstDiscFirstTieTrack, trackOfAnotherAlbum);
        await _mockContext.SaveChangesAsync();

        List<TrackEntity> albumTracks = [firstDiscFirstTrack, firstDiscFirstTieTrack, firstDiscSecondTrack, secondDiscTrack];
        Guid[] expectedOrder = [.. albumTracks
            .OrderBy(track => track.DiscNumber)
            .ThenBy(track => track.TrackNumber)
            .ThenBy(track => track.Id)
            .Select(track => track.Id)];

        // Act
        Result<IReadOnlyList<TrackEntity>> result = await _sut.GetByAlbumIdAsync(albumId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedOrder, result.Value.Select(track => track.Id));
        Assert.DoesNotContain(result.Value, track => track.Id == trackOfAnotherAlbum.Id);
    }

    [Fact]
    public async Task GetByAlbumIdAsync_WhenCalled_ShouldIncludeTheRelatedCollections()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(albumId: albumId, path: "/music/queen/radio-ga-ga.flac", includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "Tag1"), _tagEntityFixture.Create(name: "Tag2")];
        track.Genres = [_genreEntityFixture.Create(name: "Genre1"), _genreEntityFixture.Create(name: "Genre2")];
        track.Moods = [_trackMoodEntityFixture.Create(name: "Calm")];
        track.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678")];
        track.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100)];
        track.Contributors = [_trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)];
        _mockContext.Tracks.Add(track);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<TrackEntity>> result = await _sut.GetByAlbumIdAsync(albumId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity retrievedTrack = Assert.Single(result.Value);
        Assert.Equal(2, retrievedTrack.Tags.Count);
        Assert.Equal(2, retrievedTrack.Genres.Count);
        Assert.Single(retrievedTrack.Moods);
        Assert.Single(retrievedTrack.Isrcs);
        Assert.Single(retrievedTrack.Ratings);
        Assert.Single(retrievedTrack.Contributors);
    }

    [Fact]
    public async Task GetTracksLiteByAlbumIdAsync_WhenPaginationDataIsNull_ShouldReturnAllTracksOfTheAlbumOrdered()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        TrackEntity secondTrack = _trackEntityFixture.Create(albumId: albumId, path: "/music/queen/we-are-the-champions.flac", trackNumber: 2, title: "We Are the Champions", includeMetadata: false);
        secondTrack.DiscNumber = 1;
        TrackEntity firstTrack = _trackEntityFixture.Create(albumId: albumId, path: "/music/queen/i-want-to-break-free.flac", trackNumber: 1, title: "Bohemian Rhapsody", includeMetadata: false);
        firstTrack.DiscNumber = 1;
        TrackEntity trackOfAnotherAlbum = _trackEntityFixture.Create(path: "/music/queen/a-kind-of-magic.flac", includeMetadata: false);
        _mockContext.Tracks.AddRange(secondTrack, firstTrack, trackOfAnotherAlbum);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<TrackLiteRow>> result = await _sut.GetTracksLiteByAlbumIdAsync(albumId, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        PaginatedResultDto<TrackLiteRow> page = result.Value;
        Assert.Equal(2, page.Count);
        Assert.Equal(2, page.Data.Count);
        Assert.Equal(1, page.CurrentPage);
        Assert.Equal(2, page.PerPage);
        Assert.Equal(1, page.NumberOfPages);
        Assert.Equal([firstTrack.Id, secondTrack.Id], page.Data.Select(row => row.Id));
    }

    [Theory]
    [InlineData(1, 2, 1, 2)] // The first page is fully returned.
    [InlineData(2, 2, 2, 2)] // A middle page is fully returned.
    [InlineData(3, 2, 3, 1)] // The last page is partially returned.
    [InlineData(99, 2, 3, 1)] // A page beyond the last one is clamped to the last page.
    public async Task GetTracksLiteByAlbumIdAsync_WhenPaginationIsProvided_ShouldReturnTheRequestedPage(int requestedPage, int perPage, int expectedCurrentPage, int expectedRowCount)
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        List<TrackEntity> tracks = _trackEntityFixture.CreateMany(5);
        for (int index = 0; index < tracks.Count; index++)
        {
            tracks[index].AlbumId = albumId;
            tracks[index].DiscNumber = 1;
            tracks[index].TrackNumber = index + 1;
            tracks[index].Path = $"/music/queen/track-{index}.flac";
            tracks[index].Tags = [];
            tracks[index].Genres = [];
            tracks[index].Moods = [];
            tracks[index].Isrcs = [];
            tracks[index].Contributors = [];
            tracks[index].Ratings = [];
        }
        _mockContext.Tracks.AddRange(tracks);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: requestedPage, perPage: perPage);

        // Act
        Result<PaginatedResultDto<TrackLiteRow>> result = await _sut.GetTracksLiteByAlbumIdAsync(albumId, paginationData, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        PaginatedResultDto<TrackLiteRow> page = result.Value;
        Assert.Equal(5, page.Count);
        Assert.Equal(3, page.NumberOfPages);
        Assert.Equal(expectedCurrentPage, page.CurrentPage);
        Assert.Equal(perPage, page.PerPage);
        Assert.Equal(expectedRowCount, page.Data.Count);
    }

    [Fact]
    public async Task GetTracksLiteByAlbumIdAsync_WhenCalled_ShouldReturnOnlyTheTracksOfTheRequestedAlbum()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        TrackEntity trackOfAlbum = _trackEntityFixture.Create(albumId: albumId, path: "/music/queen/youre-my-best-friend.flac", includeMetadata: false);
        TrackEntity trackOfAnotherAlbum = _trackEntityFixture.Create(path: "/music/queen/fat-bottomed-girls.flac", includeMetadata: false);
        _mockContext.Tracks.AddRange(trackOfAlbum, trackOfAnotherAlbum);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);

        // Act
        Result<PaginatedResultDto<TrackLiteRow>> result = await _sut.GetTracksLiteByAlbumIdAsync(albumId, paginationData, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TrackLiteRow row = Assert.Single(result.Value.Data);
        Assert.Equal(trackOfAlbum.Id, row.Id);
        Assert.Equal(1, result.Value.Count);
    }

    [Fact]
    public async Task GetTracksLiteByAlbumIdAsync_WhenCalled_ShouldProjectTheLiteFields()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(albumId: albumId, path: "/music/queen/bicycle-race.flac", title: "Killer Queen", trackNumber: 9, includeMetadata: false);
        track.DiscNumber = 3;
        _mockContext.Tracks.Add(track);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);

        // Act
        Result<PaginatedResultDto<TrackLiteRow>> result = await _sut.GetTracksLiteByAlbumIdAsync(albumId, paginationData, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TrackLiteRow row = Assert.Single(result.Value.Data);
        Assert.Equal(track.Id, row.Id);
        Assert.Equal("Killer Queen", row.Title);
        Assert.Equal(9, row.TrackNumber);
        Assert.Equal(3, row.DiscNumber);
    }
}
