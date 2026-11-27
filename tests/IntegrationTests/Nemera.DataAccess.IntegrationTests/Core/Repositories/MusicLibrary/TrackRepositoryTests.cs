#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Common;
using Nemera.Application.Common.DataAccess.Entities.MediaContributors;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Nemera.Application.Common.DTO.Pagination;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Time;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.Common;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Fixtures.Common.DTO.Pagination;
using Nemera.DataAccess.Common.Interceptors;
using Nemera.DataAccess.Core.Repositories.MusicLibrary;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Nemera.Domain.SharedKernel.Common.Enums.MediaContributors;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.DataAccess.IntegrationTests.Core.Repositories.MusicLibrary;

/// <summary>
/// Contains integration tests for the <see cref="TrackRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackRepositoryTests
{
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly TrackMoodEntityFixture _trackMoodEntityFixture = new();
    private readonly TrackIsrcEntityFixture _trackIsrcEntityFixture = new();
    private readonly TrackContributorEntityFixture _trackContributorEntityFixture = new();
    private readonly AudioRatingEntityFixture _audioRatingEntityFixture = new();
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly GenreEntityFixture _genreEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();
    private readonly PaginationDataDtoFixture _paginationDataDtoFixture = new();
    private readonly IDateTimeProvider _mockDateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackRepositoryTests"/> class.
    /// </summary>
    public TrackRepositoryTests()
    {
        _mockDateTimeProvider = Substitute.For<IDateTimeProvider>();
        _mockDateTimeProvider.UtcNow.Returns(new DateTime(2026, 11, 14, 10, 30, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task InsertAsync_WhenCalledWithAValidTrack_ShouldPersistTheTrackAndItsChildren()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-insert-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        MediaContributorEntity performer = _mediaContributorEntityFixture.Create(displayName: $"Performer {Guid.NewGuid()}");

        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "InsertTag")];
        track.Genres = [_genreEntityFixture.Create(name: "InsertGenre")];
        track.Moods = [_trackMoodEntityFixture.Create(name: "Calm")];
        track.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678")];
        track.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100)];
        track.Contributors = [_trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: performer.Id, role: MediaContributorRole.Vocals)];

        // Act
        Result<Created> result = await sut.InsertAsync(track, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        TrackEntity? storedTrack = await context.Tracks
            .AsNoTracking()
            .Include(candidate => candidate.Tags)
            .Include(candidate => candidate.Genres)
            .Include(candidate => candidate.Moods)
            .Include(candidate => candidate.Isrcs)
            .Include(candidate => candidate.Ratings)
            .Include(candidate => candidate.Contributors)
            .FirstOrDefaultAsync(candidate => candidate.Id == track.Id);
        Assert.NotNull(storedTrack);
        Assert.Equal("/music/queen/bohemian-rhapsody.flac", storedTrack!.Path);
        Assert.Single(storedTrack.Tags);
        Assert.Single(storedTrack.Genres);
        Assert.Single(storedTrack.Moods);
        Assert.Single(storedTrack.Isrcs);
        Assert.Single(storedTrack.Ratings);
        TrackContributorEntity storedContributor = Assert.Single(storedTrack.Contributors);
        Assert.Equal(performer.Id, storedContributor.MediaContributorId);
    }

    [Fact]
    public async Task InsertAsync_WhenAnotherTrackOfTheSameLibraryHasTheSamePath_ShouldReturnTrackAlreadyExists()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-insert-conflict-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity existingTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/youre-my-best-friend.flac", includeMetadata: false);
        context.Tracks.Add(existingTrack);
        await context.SaveChangesAsync();

        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/youre-my-best-friend.flac", includeMetadata: false);

        // Act
        Result<Created> result = await sut.InsertAsync(track, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task InsertAsync_WhenAnotherLibraryHasATrackWithTheSamePath_ShouldPersistTheTrack()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-insert-otherlibrary-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = CreateTrackRepository(context);

        Guid firstLibraryId = Guid.NewGuid();
        AlbumEntity firstAlbum = await SeedAlbumAsync(context, firstLibraryId);
        TrackEntity existingTrack = _trackEntityFixture.Create(albumId: firstAlbum.Id, libraryId: firstLibraryId, path: "/music/queen/we-will-rock-you.flac", includeMetadata: false);
        context.Tracks.Add(existingTrack);
        await context.SaveChangesAsync();

        Guid secondLibraryId = Guid.NewGuid();
        AlbumEntity secondAlbum = await SeedAlbumAsync(context, secondLibraryId);
        TrackEntity track = _trackEntityFixture.Create(albumId: secondAlbum.Id, libraryId: secondLibraryId, path: "/music/queen/we-will-rock-you.flac", includeMetadata: false);

        // Act
        Result<Created> result = await sut.InsertAsync(track, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        TrackEntity? storedTrack = await context.Tracks.AsNoTracking().FirstOrDefaultAsync(candidate => candidate.Id == track.Id);
        Assert.NotNull(storedTrack);
        Assert.Equal(secondLibraryId, storedTrack!.LibraryId);
    }

    [Fact]
    public async Task GetExistingPathsAsync_WhenCalled_ShouldReturnOnlyThePathsOfTheLibraryCaseSensitively()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-existingpaths-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity trackOfLibrary = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/Queen/Love-Of-My-Life.flac", includeMetadata: false);
        context.Tracks.Add(trackOfLibrary);

        Guid anotherLibraryId = Guid.NewGuid();
        AlbumEntity anotherAlbum = await SeedAlbumAsync(context, anotherLibraryId);
        TrackEntity trackOfAnotherLibrary = _trackEntityFixture.Create(albumId: anotherAlbum.Id, libraryId: anotherLibraryId, path: "/music/Queen/Love-Of-My-Life.flac", includeMetadata: false);
        context.Tracks.Add(trackOfAnotherLibrary);
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyCollection<string>> result = await sut.GetExistingPathsAsync(
            libraryId, ["/music/Queen/Love-Of-My-Life.flac", "/music/queen/love-of-my-life.flac", "/music/queen/we-are-the-champions.flac"], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        string existingPath = Assert.Single(result.Value);
        // The comparison is ordinal, so neither the same path of another library nor a case variant of the path is reported as existing.
        Assert.Equal("/music/Queen/Love-Of-My-Life.flac", existingPath);
    }

    [Fact]
    public async Task GetExistingPathsAsync_WhenMorePathsAreRequestedThanTheStorageAccepts_ShouldReturnTheExistingPaths()
    {
        // Arrange
        // A scan of a very large library stages more paths than SQLite accepts as the parameters of one statement, which used to fail the whole scan.
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-existingpaths-large-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false);
        context.Tracks.Add(track);
        await context.SaveChangesAsync();
        List<string> requestedPaths = [.. Enumerable.Range(0, 120_000).Select(index => $"/music/queen/track-{index}.flac")];
        requestedPaths.Add("/music/queen/bohemian-rhapsody.flac");

        // Act
        Result<IReadOnlyCollection<string>> result = await sut.GetExistingPathsAsync(libraryId, requestedPaths, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        string existingPath = Assert.Single(result.Value);
        Assert.Equal("/music/queen/bohemian-rhapsody.flac", existingPath);
    }

    [Fact]
    public async Task UpdateAsync_WhenNothingChanged_ShouldNotChangeAnyAuditColumn()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-update-noop-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using NemeraDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        MediaContributorEntity performer = _mediaContributorEntityFixture.Create(displayName: $"Performer {Guid.NewGuid()}");
        context.MediaContributors.Add(performer);
        await context.SaveChangesAsync();

        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/dont-stop-me-now.flac", includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "NoopTag")];
        track.Genres = [_genreEntityFixture.Create(name: "NoopGenre")];
        track.Moods = [_trackMoodEntityFixture.Create(name: "Calm")];
        track.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678")];
        track.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100)];
        track.Contributors =
            [_trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: performer.Id, role: MediaContributorRole.Vocals)];
        context.Tracks.Add(track);
        await context.SaveChangesAsync();
        // Detach the seeded graph, so that the update must load the tracked track through its include chain instead of returning the already populated seeded instance.
        context.ChangeTracker.Clear();
        Guid contributorId = track.Contributors[0].Id;
        DateTime contributorCreatedOnUtc = track.Contributors[0].CreatedOnUtc;
        TrackEntity incoming = await LoadDetachedTrackAsync(context, track.Id);

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity? storedTrack = await context.Tracks.AsNoTracking().Include(candidate => candidate.Contributors).FirstOrDefaultAsync(candidate => candidate.Id == track.Id);
        Assert.NotNull(storedTrack);
        Assert.Null(storedTrack!.UpdatedOnUtc);
        TrackContributorEntity storedContributor = Assert.Single(storedTrack.Contributors);
        Assert.Equal(contributorId, storedContributor.Id);
        Assert.Equal(contributorCreatedOnUtc, storedContributor.CreatedOnUtc);
        Assert.Single(context.Set<TagEntity>().Where(tag => tag.Name == "NoopTag"));
        Assert.Single(context.Set<GenreEntity>().Where(genre => genre.Name == "NoopGenre"));
    }

    [Fact]
    public async Task UpdateAsync_WhenOnlyTheMetadataChanged_ShouldStampTheTrackAndKeepTheChildRowsUntouched()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-update-metadata-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using NemeraDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        MediaContributorEntity performer = _mediaContributorEntityFixture.Create(displayName: $"Performer {Guid.NewGuid()}");
        context.MediaContributors.Add(performer);
        await context.SaveChangesAsync();

        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/somebody-to-love.flac", includeMetadata: false);
        track.Contributors =
            [_trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: performer.Id, role: MediaContributorRole.Vocals)];
        context.Tracks.Add(track);
        await context.SaveChangesAsync();
        Guid contributorId = track.Contributors[0].Id;
        DateTime contributorCreatedOnUtc = track.Contributors[0].CreatedOnUtc;
        TrackEntity incoming = await LoadDetachedTrackAsync(context, track.Id);
        incoming.Title = "Somebody to Love";

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity? storedTrack = await context.Tracks.AsNoTracking().Include(candidate => candidate.Contributors).FirstOrDefaultAsync(candidate => candidate.Id == track.Id);
        Assert.NotNull(storedTrack);
        Assert.Equal("Somebody to Love", storedTrack!.Title);
        // Only the edited track is stamped, the child rows keep their identity and their audit columns.
        Assert.Equal(utcNow, storedTrack.UpdatedOnUtc);
        Assert.Equal(userId, storedTrack.UpdatedBy);
        TrackContributorEntity storedContributor = Assert.Single(storedTrack.Contributors);
        Assert.Equal(contributorId, storedContributor.Id);
        Assert.Equal(contributorCreatedOnUtc, storedContributor.CreatedOnUtc);
    }

    [Fact]
    public async Task UpdateAsync_WhenAContributorIsRemoved_ShouldDeleteOnlyThatParticipationAndKeepTheOtherIdentity()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-update-removecontributor-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using NemeraDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        Guid keptMediaContributorId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/another-one-bites-the-dust.flac", includeMetadata: false);
        TrackContributorEntity keptContributor = _trackContributorEntityFixture.Create(
            trackId: track.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Composer);
        TrackContributorEntity removedContributor = _trackContributorEntityFixture.Create(
            trackId: track.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Producer);
        track.Contributors = [keptContributor, removedContributor];
        context.Tracks.Add(track);
        await context.SaveChangesAsync();
        Guid keptContributorId = keptContributor.Id;
        TrackEntity incoming = await LoadDetachedTrackAsync(context, track.Id);
        incoming.Contributors = [incoming.Contributors.Single(contributor => contributor.MediaContributorId == keptMediaContributorId)];

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        List<TrackContributorEntity> storedContributors = await context.Set<TrackContributorEntity>()
            .AsNoTracking()
            .Where(contributor => contributor.TrackId == track.Id)
            .ToListAsync();
        TrackContributorEntity storedContributor = Assert.Single(storedContributors);
        Assert.Equal(keptContributorId, storedContributor.Id);
        Assert.Equal(keptMediaContributorId, storedContributor.MediaContributorId);
    }

    [Fact]
    public async Task UpdateAsync_WhenAContributorIsAdded_ShouldInsertOnlyTheNewParticipationAndKeepTheExistingIdentity()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-update-addcontributor-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using NemeraDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        Guid keptMediaContributorId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/crazy-little-thing-called-love.flac", includeMetadata: false);
        TrackContributorEntity keptContributor = _trackContributorEntityFixture.Create(
            trackId: track.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Composer);
        track.Contributors = [keptContributor];
        context.Tracks.Add(track);
        await context.SaveChangesAsync();
        Guid keptContributorId = keptContributor.Id;
        Guid addedMediaContributorId = Guid.NewGuid();

        TrackEntity incoming = await LoadDetachedTrackAsync(context, track.Id);
        incoming.Contributors.Add(_trackContributorEntityFixture.Create(
            trackId: track.Id, mediaContributorId: addedMediaContributorId, role: MediaContributorRole.Vocals));

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        List<TrackContributorEntity> storedContributors = await context.Set<TrackContributorEntity>()
            .AsNoTracking()
            .Where(contributor => contributor.TrackId == track.Id)
            .ToListAsync();
        Assert.Equal(2, storedContributors.Count);
        Assert.Equal(keptContributorId, storedContributors.Single(contributor => contributor.MediaContributorId == keptMediaContributorId).Id);
        Assert.Contains(storedContributors, contributor => contributor.MediaContributorId == addedMediaContributorId);
    }

    [Fact]
    public async Task UpdateAsync_WhenDataContainsExistingTagAndGenreNames_ShouldReuseTheStoredTagAndGenreEntities()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-update-tags-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using NemeraDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/radio-ga-ga.flac", includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "ExistingTag")];
        track.Genres = [_genreEntityFixture.Create(name: "ExistingGenre")];
        context.Tracks.Add(track);
        await context.SaveChangesAsync();

        TrackEntity incoming = await LoadDetachedTrackAsync(context, track.Id);
        incoming.Tags = [_tagEntityFixture.Create(name: "ExistingTag")];
        incoming.Genres = [_genreEntityFixture.Create(name: "ExistingGenre")];

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity? storedTrack = await context.Tracks
            .AsNoTracking()
            .Include(candidate => candidate.Tags)
            .Include(candidate => candidate.Genres)
            .FirstOrDefaultAsync(candidate => candidate.Id == track.Id);
        Assert.NotNull(storedTrack);
        Assert.Equal("ExistingTag", Assert.Single(storedTrack!.Tags).Name);
        Assert.Equal("ExistingGenre", Assert.Single(storedTrack.Genres).Name);
        // The shared rows are reused instead of being duplicated by the update.
        Assert.Single(context.Set<TagEntity>().Where(tag => tag.Name == "ExistingTag"));
        Assert.Single(context.Set<GenreEntity>().Where(genre => genre.Name == "ExistingGenre"));
    }

    [Fact]
    public async Task UpdateAsync_WhenOwnedAudioCollectionsChange_ShouldReconcileRatingsMoodsAndIsrcs()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-update-owned-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using NemeraDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/the-show-must-go-on.flac", includeMetadata: false);
        track.Ratings =
        [
            _audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100),
            _audioRatingEntityFixture.Create(value: 5, maxValue: 5, source: AudioRatingSource.User, voteCount: 10),
            _audioRatingEntityFixture.Create(value: 2, maxValue: 5, source: AudioRatingSource.LastFm, voteCount: 20)
        ];
        track.Moods = [_trackMoodEntityFixture.Create(name: "Calm"), _trackMoodEntityFixture.Create(name: "Energetic")];
        track.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678"), _trackIsrcEntityFixture.Create(value: "GBAYE0000001")];
        context.Tracks.Add(track);
        await context.SaveChangesAsync();

        TrackEntity incoming = await LoadDetachedTrackAsync(context, track.Id);
        incoming.Ratings =
        [
            _audioRatingEntityFixture.Create(value: 3, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100),
            _audioRatingEntityFixture.Create(value: 5, maxValue: 5, source: AudioRatingSource.User, voteCount: 10),
            _audioRatingEntityFixture.Create(value: 1, maxValue: 5, source: AudioRatingSource.Discogs, voteCount: 5)
        ];
        incoming.Moods = [_trackMoodEntityFixture.Create(name: "Calm"), _trackMoodEntityFixture.Create(name: "Upbeat")];
        incoming.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678"), _trackIsrcEntityFixture.Create(value: "GBAYE0000002")];

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity? storedTrack = await context.Tracks
            .AsNoTracking()
            .Include(candidate => candidate.Ratings)
            .Include(candidate => candidate.Moods)
            .Include(candidate => candidate.Isrcs)
            .FirstOrDefaultAsync(candidate => candidate.Id == track.Id);
        Assert.NotNull(storedTrack);
        Assert.Equal(3, storedTrack!.Ratings.Count);
        Assert.Contains(storedTrack.Ratings, rating => rating.Source == AudioRatingSource.MusicBrainz && rating.Value == 3M);
        Assert.Contains(storedTrack.Ratings, rating => rating.Source == AudioRatingSource.User && rating.Value == 5M);
        Assert.Contains(storedTrack.Ratings, rating => rating.Source == AudioRatingSource.Discogs && rating.Value == 1M);
        Assert.DoesNotContain(storedTrack.Ratings, rating => rating.Source == AudioRatingSource.LastFm);
        Assert.Equal(2, storedTrack.Moods.Count);
        Assert.Single(storedTrack.Moods, mood => mood.Name == "Calm");
        Assert.Contains(storedTrack.Moods, mood => mood.Name == "Upbeat");
        Assert.Equal(2, storedTrack.Isrcs.Count);
        Assert.Single(storedTrack.Isrcs, isrc => isrc.Value == "USRC12345678");
        Assert.Contains(storedTrack.Isrcs, isrc => isrc.Value == "GBAYE0000002");
    }

    [Fact]
    public async Task GetByIdAsync_WhenTrackExists_ShouldIncludeTheNavigationProperties()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getbyid-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        ArtistEntity artist = await context.Artists.AsNoTracking().FirstAsync(candidate => candidate.Id == album.ArtistId);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/killer-queen.flac", includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "Tag1"), _tagEntityFixture.Create(name: "Tag2")];
        track.Genres = [_genreEntityFixture.Create(name: "Genre1"), _genreEntityFixture.Create(name: "Genre2")];
        track.Moods = [_trackMoodEntityFixture.Create(name: "Calm"), _trackMoodEntityFixture.Create(name: "Upbeat")];
        track.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678")];
        track.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100)];
        track.Contributors = [_trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)];
        context.Tracks.Add(track);
        await context.SaveChangesAsync();

        // Act
        Result<TrackEntity?> result = await sut.GetByIdAsync(track.Id, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity retrievedTrack = result.Value!;
        Assert.NotNull(retrievedTrack.Album);
        Assert.Equal(album.Id, retrievedTrack.Album!.Id);
        Assert.NotNull(retrievedTrack.Album.Artist);
        Assert.Equal(artist.Id, retrievedTrack.Album.Artist!.Id);
        Assert.Equal(2, retrievedTrack.Tags.Count);
        Assert.Equal(2, retrievedTrack.Genres.Count);
        Assert.Equal(2, retrievedTrack.Moods.Count);
        Assert.Single(retrievedTrack.Isrcs);
        Assert.Single(retrievedTrack.Ratings);
        Assert.Single(retrievedTrack.Contributors);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntitiesShouldNotBeTracked_ShouldReturnAnUntrackedTrack()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getbyid-notracking-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/seven-seas-of-rhye.flac", includeMetadata: false);
        context.Tracks.Add(track);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<TrackEntity?> result = await sut.GetByIdAsync(
            track.Id, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(context.ChangeTracker.Entries<TrackEntity>());
    }

    [Fact]
    public async Task GetByAlbumIdAsync_WhenCalled_ShouldReturnTheAlbumTracksInOrderWithTheirCollections()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getbyalbum-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity secondDiscTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/stone-cold-crazy.flac", trackNumber: 1, includeMetadata: false);
        secondDiscTrack.DiscNumber = 2;
        TrackEntity firstDiscSecondTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/good-old-fashioned-lover-boy.flac", trackNumber: 2, includeMetadata: false);
        firstDiscSecondTrack.DiscNumber = 1;
        TrackEntity firstDiscFirstTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/tie-your-mother-down.flac", trackNumber: 1, includeMetadata: false);
        firstDiscFirstTrack.DiscNumber = 1;
        firstDiscFirstTrack.Tags = [_tagEntityFixture.Create(name: "OrderedTag")];
        firstDiscFirstTrack.Genres = [_genreEntityFixture.Create(name: "OrderedGenre")];
        firstDiscFirstTrack.Moods = [_trackMoodEntityFixture.Create(name: "Calm")];
        firstDiscFirstTrack.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678")];
        firstDiscFirstTrack.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100)];
        firstDiscFirstTrack.Contributors = [_trackContributorEntityFixture.Create(trackId: firstDiscFirstTrack.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)];
        context.Tracks.AddRange(secondDiscTrack, firstDiscSecondTrack, firstDiscFirstTrack);

        AlbumEntity anotherAlbum = await SeedAlbumAsync(context, libraryId);
        TrackEntity trackOfAnotherAlbum = _trackEntityFixture.Create(albumId: anotherAlbum.Id, libraryId: libraryId, path: "/music/queen/play-the-game.flac", includeMetadata: false);
        context.Tracks.Add(trackOfAnotherAlbum);
        await context.SaveChangesAsync();

        List<TrackEntity> albumTracks = [firstDiscFirstTrack, firstDiscSecondTrack, secondDiscTrack];
        Guid[] expectedOrder = [.. albumTracks
            .OrderBy(track => track.DiscNumber ?? 1)
            .ThenBy(track => track.TrackNumber)
            .ThenBy(track => track.Id)
            .Select(track => track.Id)];

        // Act
        Result<IReadOnlyList<TrackEntity>> result = await sut.GetByAlbumIdAsync(album.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedOrder, result.Value.Select(track => track.Id));
        Assert.DoesNotContain(result.Value, track => track.Id == trackOfAnotherAlbum.Id);
        TrackEntity retrievedTrack = result.Value.First(track => track.Id == firstDiscFirstTrack.Id);
        Assert.Single(retrievedTrack.Tags);
        Assert.Single(retrievedTrack.Genres);
        Assert.Single(retrievedTrack.Moods);
        Assert.Single(retrievedTrack.Isrcs);
        Assert.Single(retrievedTrack.Ratings);
        Assert.Single(retrievedTrack.Contributors);
    }

    [Fact]
    public async Task GetTracksLiteByAlbumIdAsync_WhenPaginationDataIsNull_ShouldReturnAllTracksOfTheAlbum()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-lite-null-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        List<TrackEntity> tracks = _trackEntityFixture.CreateMany(3);
        for (int index = 0; index < tracks.Count; index++)
        {
            tracks[index].AlbumId = album.Id;
            tracks[index].LibraryId = libraryId;
            tracks[index].Path = $"/music/queen/track-{index}.flac";
            tracks[index].DiscNumber = 1;
            tracks[index].TrackNumber = index + 1;
            tracks[index].Tags = [];
            tracks[index].Genres = [];
            tracks[index].Moods = [];
            tracks[index].Isrcs = [];
            tracks[index].Ratings = [];
            tracks[index].Contributors = [];
        }
        context.Tracks.AddRange(tracks);

        AlbumEntity anotherAlbum = await SeedAlbumAsync(context, libraryId);
        TrackEntity trackOfAnotherAlbum = _trackEntityFixture.Create(albumId: anotherAlbum.Id, libraryId: libraryId, path: "/music/queen/save-me.flac", includeMetadata: false);
        context.Tracks.Add(trackOfAnotherAlbum);
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<TrackLiteRow>> result = await sut.GetTracksLiteByAlbumIdAsync(album.Id, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        PaginatedResultDto<TrackLiteRow> page = result.Value;
        Assert.Equal(3, page.Count);
        Assert.Equal(3, page.Data.Count);
        Assert.Equal(1, page.CurrentPage);
        Assert.Equal(3, page.PerPage);
        Assert.Equal(1, page.NumberOfPages);
        Assert.DoesNotContain(page.Data, row => row.Id == trackOfAnotherAlbum.Id);
        Assert.Equal(tracks.Select(track => track.DurationInSeconds), page.Data.Select(row => row.DurationInSeconds));
    }

    [Fact]
    public async Task GetTracksLiteByAlbumIdAsync_WhenATrackHasNoDiscNumber_ShouldTreatItAsTheFirstDisc()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-lite-nodisc-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity secondTrackWithoutDiscNumber = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/second.flac", trackNumber: 2, includeMetadata: false);
        secondTrackWithoutDiscNumber.DiscNumber = null;
        TrackEntity thirdTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/third.flac", trackNumber: 3, includeMetadata: false);
        thirdTrack.DiscNumber = 1;
        context.Tracks.AddRange(secondTrackWithoutDiscNumber, thirdTrack);
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<TrackLiteRow>> result = await sut.GetTracksLiteByAlbumIdAsync(album.Id, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal([secondTrackWithoutDiscNumber.Id, thirdTrack.Id], result.Value.Data.Select(row => row.Id));
    }

    [Theory]
    [InlineData(1, 2, 1, 2)] // The first page is fully returned.
    [InlineData(2, 2, 2, 2)] // A middle page is fully returned.
    [InlineData(3, 2, 3, 1)] // The last page is partially returned.
    [InlineData(99, 2, 3, 1)] // A page beyond the last one is clamped to the last page.
    public async Task GetTracksLiteByAlbumIdAsync_WhenPaginationIsProvided_ShouldReturnTheRequestedPage(
        int requestedPage, int perPage, int expectedCurrentPage, int expectedRowCount)
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-lite-page-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        List<TrackEntity> tracks = _trackEntityFixture.CreateMany(5);
        for (int index = 0; index < tracks.Count; index++)
        {
            tracks[index].AlbumId = album.Id;
            tracks[index].LibraryId = libraryId;
            tracks[index].Path = $"/music/queen/song-{index}.flac";
            tracks[index].DiscNumber = 1;
            tracks[index].TrackNumber = index + 1;
            tracks[index].Tags = [];
            tracks[index].Genres = [];
            tracks[index].Moods = [];
            tracks[index].Isrcs = [];
            tracks[index].Ratings = [];
            tracks[index].Contributors = [];
        }
        context.Tracks.AddRange(tracks);
        await context.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: requestedPage, perPage: perPage);

        // Act
        Result<PaginatedResultDto<TrackLiteRow>> result = await sut.GetTracksLiteByAlbumIdAsync(album.Id, paginationData, CancellationToken.None);

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
    public async Task GetByAlbumIdAsync_WhenATrackHasNoDiscNumber_ShouldTreatItAsTheFirstDisc()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-nodisc-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity secondDiscTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/second-disc.flac", trackNumber: 1, includeMetadata: false);
        secondDiscTrack.DiscNumber = 2;
        TrackEntity trackWithoutDiscNumber = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/first-disc.flac", trackNumber: 2, includeMetadata: false);
        trackWithoutDiscNumber.DiscNumber = null;
        context.Tracks.AddRange(secondDiscTrack, trackWithoutDiscNumber);
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<TrackEntity>> result = await sut.GetByAlbumIdAsync(album.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        // A track without a disc number belongs to the first disc, so it is ordered before the tracks of disc 2.
        Assert.Equal([trackWithoutDiscNumber.Id, secondDiscTrack.Id], result.Value.Select(track => track.Id));
    }

    [Fact]
    public async Task GetTracksLiteByAlbumIdAsync_WhenCalled_ShouldProjectTheDurationInSeconds()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-lite-duration-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = CreateTrackRepository(context);

        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/the-prophet-song.flac", includeMetadata: false);
        track.DurationInSeconds = 367;
        context.Tracks.Add(track);
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<TrackLiteRow>> result = await sut.GetTracksLiteByAlbumIdAsync(album.Id, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TrackLiteRow row = Assert.Single(result.Value.Data);
        Assert.Equal(367, row.DurationInSeconds);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenIdsIsEmpty_ShouldReturnAnEmptyList()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getbyids-empty-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        context.Tracks.Add(_trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false));
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<TrackEntity>> result = await sut.GetByIdsAsync([], false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenCalled_ShouldReturnOnlyTheRequestedTracks()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getbyids-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity firstTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/first.flac", includeMetadata: false);
        TrackEntity secondTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/second.flac", includeMetadata: false);
        TrackEntity thirdTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/third.flac", includeMetadata: false);
        context.Tracks.AddRange(firstTrack, secondTrack, thirdTrack);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyList<TrackEntity>> result = await sut.GetByIdsAsync([firstTrack.Id, thirdTrack.Id], false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, track => track.Id == firstTrack.Id);
        Assert.Contains(result.Value, track => track.Id == thirdTrack.Id);
        Assert.DoesNotContain(result.Value, track => track.Id == secondTrack.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenNavigationPropertiesShouldNotBeIncluded_ShouldNotLoadTheNonOwnedNavigationProperties()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getbyids-notracking-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/hidden.flac", includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "HiddenTag")];
        track.Genres = [_genreEntityFixture.Create(name: "HiddenGenre")];
        track.Moods = [_trackMoodEntityFixture.Create(name: "HiddenMood")];
        track.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678")];
        track.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100)];
        track.Contributors = [_trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)];
        context.Tracks.Add(track);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyList<TrackEntity>> result = await sut.GetByIdsAsync([track.Id], false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity returnedTrack = Assert.Single(result.Value);
        Assert.Equal(track.Id, returnedTrack.Id);
        Assert.Null(returnedTrack.Album);
        Assert.Empty(returnedTrack.Tags);
        Assert.Empty(returnedTrack.Genres);
        Assert.Empty(returnedTrack.Contributors);
        // Moods, ISRCs and Ratings are owned collections, so EF Core materializes them together with the track even when the optional graph
        // is not included. The flag only controls the non-owned navigation properties, which is why those are asserted unloaded above.
        Assert.Single(returnedTrack.Moods);
        Assert.Single(returnedTrack.Isrcs);
        Assert.Single(returnedTrack.Ratings);
        // The rows are read without tracking, so the read statistics never leak into the unit of work.
        Assert.Empty(context.ChangeTracker.Entries<TrackEntity>());
        Assert.Equal(EntityState.Detached, context.Entry(returnedTrack).State);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenNavigationPropertiesShouldBeIncluded_ShouldLoadTheAlbumArtistAndRelatedCollections()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getbyids-include-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId, includeMetadata: true);
        ArtistEntity artist = await context.Artists.AsNoTracking().FirstAsync(candidate => candidate.Id == album.ArtistId);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/loaded.flac", includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "LoadedTag"), _tagEntityFixture.Create(name: "SecondLoadedTag")];
        track.Genres = [_genreEntityFixture.Create(name: "LoadedGenre")];
        track.Moods = [_trackMoodEntityFixture.Create(name: "Calm")];
        track.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678")];
        track.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100)];
        track.Contributors = [_trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)];
        context.Tracks.Add(track);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyList<TrackEntity>> result = await sut.GetByIdsAsync([track.Id], true, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity returnedTrack = Assert.Single(result.Value);
        Assert.NotNull(returnedTrack.Album);
        Assert.Equal(album.Id, returnedTrack.Album!.Id);
        Assert.NotNull(returnedTrack.Album.Artist);
        Assert.Equal(artist.Id, returnedTrack.Album.Artist!.Id);
        Assert.Single(returnedTrack.Album.Tags);
        Assert.Single(returnedTrack.Album.Genres);
        Assert.Single(returnedTrack.Album.Artist.Tags);
        Assert.Single(returnedTrack.Album.Artist.Genres);
        Assert.Equal(2, returnedTrack.Tags.Count);
        Assert.Single(returnedTrack.Genres);
        Assert.Single(returnedTrack.Moods);
        Assert.Single(returnedTrack.Isrcs);
        Assert.Single(returnedTrack.Ratings);
        Assert.Single(returnedTrack.Contributors);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenMoreThanFiveHundredIdsAreRequested_ShouldReturnEveryTrack()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getbyids-chunks-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        const int TRACK_COUNT = 501;
        List<TrackEntity> tracks = CreateTracks(TRACK_COUNT, album.Id, libraryId);
        context.Tracks.AddRange(tracks);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        // The lookup is chunked in batches of 500, so the 501st Id proves the second chunk is queried as well.
        Result<IReadOnlyList<TrackEntity>> result = await sut.GetByIdsAsync([.. tracks.Select(track => track.Id)], false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(TRACK_COUNT, result.Value.Count);
        Assert.Equal(
            tracks.Select(track => track.Id).OrderBy(id => id),
            result.Value.Select(track => track.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task GetShuffleTracksLiteAsync_WhenCalled_ShouldReturnOnlyTracksOfTheRequestedLibrary()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-shuffle-library-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        List<TrackEntity> libraryTracks = await SeedShuffleTracksAsync(context, album, libraryId, 3);
        Guid otherLibraryId = Guid.NewGuid();
        ArtistEntity otherArtist = await SeedArtistAsync(context, otherLibraryId, "ABBA");
        AlbumEntity otherAlbum = await SeedAlbumForArtistAsync(context, otherLibraryId, otherArtist.Id, "Arrival");
        List<TrackEntity> otherLibraryTracks = await SeedShuffleTracksAsync(context, otherAlbum, otherLibraryId, 3);

        // Act
        Result<IReadOnlyList<ShuffleTrackRowDto>> result = await sut.GetShuffleTracksLiteAsync(
            libraryId, null, null, [], 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(libraryTracks.Count, result.Value.Count);
        Assert.All(result.Value, row => Assert.Contains(libraryTracks, track => track.Id == row.TrackId));
        Assert.DoesNotContain(result.Value, row => otherLibraryTracks.Exists(track => track.Id == row.TrackId));
        Assert.Equal(result.Value.Count, result.Value.Select(row => row.TrackId).Distinct().Count());
    }

    [Fact]
    public async Task GetShuffleTracksLiteAsync_WhenArtistIdIsProvided_ShouldReturnOnlyTracksOfThatArtist()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-shuffle-artist-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity artistAlbum = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        List<TrackEntity> artistTracks = await SeedShuffleTracksAsync(context, artistAlbum, libraryId, 2);
        ArtistEntity otherArtist = await SeedArtistAsync(context, libraryId, "David Bowie");
        AlbumEntity otherArtistAlbum = await SeedAlbumForArtistAsync(context, libraryId, otherArtist.Id, "Heroes");
        List<TrackEntity> otherArtistTracks = await SeedShuffleTracksAsync(context, otherArtistAlbum, libraryId, 2);

        // Act
        Result<IReadOnlyList<ShuffleTrackRowDto>> result = await sut.GetShuffleTracksLiteAsync(
            libraryId, artist.Id, null, [], 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(artistTracks.Count, result.Value.Count);
        Assert.All(result.Value, row => Assert.Contains(artistTracks, track => track.Id == row.TrackId));
        Assert.All(result.Value, row => Assert.Equal(artist.Id, row.ArtistId));
        Assert.DoesNotContain(result.Value, row => otherArtistTracks.Exists(track => track.Id == row.TrackId));
    }

    [Fact]
    public async Task GetShuffleTracksLiteAsync_WhenAlbumIdIsProvided_ShouldReturnOnlyTracksOfThatAlbum()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-shuffle-album-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        List<TrackEntity> albumTracks = await SeedShuffleTracksAsync(context, album, libraryId, 2);
        AlbumEntity otherAlbum = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "News of the World");
        List<TrackEntity> otherAlbumTracks = await SeedShuffleTracksAsync(context, otherAlbum, libraryId, 2);

        // Act
        Result<IReadOnlyList<ShuffleTrackRowDto>> result = await sut.GetShuffleTracksLiteAsync(
            libraryId, null, album.Id, [], 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(albumTracks.Count, result.Value.Count);
        Assert.All(result.Value, row => Assert.Contains(albumTracks, track => track.Id == row.TrackId));
        Assert.All(result.Value, row => Assert.Equal(album.Id, row.AlbumId));
        Assert.DoesNotContain(result.Value, row => otherAlbumTracks.Exists(track => track.Id == row.TrackId));
    }

    [Fact]
    public async Task GetShuffleTracksLiteAsync_WhenArtistIdAndAlbumIdAreProvided_ShouldApplyBothFilters()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-shuffle-bothfilters-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        List<TrackEntity> albumTracks = await SeedShuffleTracksAsync(context, album, libraryId, 2);
        AlbumEntity otherAlbumOfArtist = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "News of the World");
        List<TrackEntity> otherAlbumOfArtistTracks = await SeedShuffleTracksAsync(context, otherAlbumOfArtist, libraryId, 2);
        ArtistEntity otherArtist = await SeedArtistAsync(context, libraryId, "David Bowie");
        AlbumEntity otherArtistAlbum = await SeedAlbumForArtistAsync(context, libraryId, otherArtist.Id, "Heroes");
        List<TrackEntity> otherArtistAlbumTracks = await SeedShuffleTracksAsync(context, otherArtistAlbum, libraryId, 2);

        // Act
        Result<IReadOnlyList<ShuffleTrackRowDto>> result = await sut.GetShuffleTracksLiteAsync(
            libraryId, artist.Id, album.Id, [], 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(albumTracks.Count, result.Value.Count);
        Assert.All(result.Value, row => Assert.Contains(albumTracks, track => track.Id == row.TrackId));
        Assert.DoesNotContain(result.Value, row => otherAlbumOfArtistTracks.Exists(track => track.Id == row.TrackId));
        Assert.DoesNotContain(result.Value, row => otherArtistAlbumTracks.Exists(track => track.Id == row.TrackId));
    }

    [Fact]
    public async Task GetShuffleTracksLiteAsync_WhenExcludedTrackIdsAreProvided_ShouldExcludeEveryOneOfThem()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-shuffle-excluded-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        List<TrackEntity> tracks = await SeedShuffleTracksAsync(context, album, libraryId, 4);
        Guid[] excludedTrackIds = [.. tracks.Take(3).Select(track => track.Id)];
        TrackEntity expectedTrack = tracks[3];

        // Act
        Result<IReadOnlyList<ShuffleTrackRowDto>> result = await sut.GetShuffleTracksLiteAsync(
            libraryId, null, null, excludedTrackIds, 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ShuffleTrackRowDto row = Assert.Single(result.Value);
        Assert.Equal(expectedTrack.Id, row.TrackId);
        Assert.DoesNotContain(result.Value, candidate => excludedTrackIds.Contains(candidate.TrackId));
    }

    [Fact]
    public async Task GetShuffleTracksLiteAsync_WhenCountIsLowerThanTheAvailableTracks_ShouldReturnAtMostCount()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-shuffle-count-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        await SeedShuffleTracksAsync(context, album, libraryId, 5);

        // Act
        Result<IReadOnlyList<ShuffleTrackRowDto>> result = await sut.GetShuffleTracksLiteAsync(
            libraryId, null, null, [], 2, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(result.Value.Count, result.Value.Select(row => row.TrackId).Distinct().Count());
    }

    [Fact]
    public async Task GetShuffleTracksLiteAsync_WhenTheLibraryHasFewerTracksThanCount_ShouldReturnEveryAvailableTrack()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-shuffle-fewertracks-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        List<TrackEntity> tracks = await SeedShuffleTracksAsync(context, album, libraryId, 2);

        // Act
        Result<IReadOnlyList<ShuffleTrackRowDto>> result = await sut.GetShuffleTracksLiteAsync(
            libraryId, null, null, [], 50, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(tracks.Count, result.Value.Count);
        Assert.All(result.Value, row => Assert.Contains(tracks, track => track.Id == row.TrackId));
    }

    [Fact]
    public async Task GetShuffleTracksLiteAsync_WhenNothingMatches_ShouldReturnAnEmptyList()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-shuffle-nomatch-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        await SeedShuffleTracksAsync(context, album, libraryId, 2);

        // Act
        Result<IReadOnlyList<ShuffleTrackRowDto>> result = await sut.GetShuffleTracksLiteAsync(
            libraryId, null, Guid.NewGuid(), [], 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetShuffleTracksLiteAsync_WhenCalled_ShouldProjectEveryField()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-shuffle-projection-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        TrackEntity track = _trackEntityFixture.Create(
            albumId: album.Id,
            libraryId: libraryId,
            path: "/music/queen/bohemian-rhapsody.flac",
            title: "Bohemian Rhapsody",
            includeMetadata: false);
        track.DurationInSeconds = 355;
        context.Tracks.Add(track);
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<ShuffleTrackRowDto>> result = await sut.GetShuffleTracksLiteAsync(
            libraryId, null, null, [], 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ShuffleTrackRowDto row = Assert.Single(result.Value);
        Assert.Equal(track.Id, row.TrackId);
        Assert.Equal("Bohemian Rhapsody", row.Title);
        Assert.Equal(355, row.DurationInSeconds);
        Assert.Equal(album.Id, row.AlbumId);
        Assert.Equal("A Night at the Opera", row.AlbumTitle);
        Assert.Equal(artist.Id, row.ArtistId);
    }

    [Fact]
    public async Task GetShuffleTracksLiteAsync_WhenTheAlbumHasACoverArtwork_ShouldProjectItsFileName()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-shuffle-cover-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        TrackEntity track = (await SeedShuffleTracksAsync(context, album, libraryId, 1))[0];
        await AddArtworkAsync(context, album.Id, ArtworkType.Cover, "a-night-at-the-opera-cover.jpg");

        // Act
        Result<IReadOnlyList<ShuffleTrackRowDto>> result = await sut.GetShuffleTracksLiteAsync(
            libraryId, null, album.Id, [], 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ShuffleTrackRowDto row = Assert.Single(result.Value);
        Assert.Equal(track.Id, row.TrackId);
        Assert.Equal("a-night-at-the-opera-cover.jpg", row.CoverPath);
    }

    [Fact]
    public async Task GetShuffleTracksLiteAsync_WhenTheAlbumHasNoArtwork_ShouldProjectANullCoverPath()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-shuffle-nocover-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        await SeedShuffleTracksAsync(context, album, libraryId, 1);

        // Act
        Result<IReadOnlyList<ShuffleTrackRowDto>> result = await sut.GetShuffleTracksLiteAsync(
            libraryId, null, album.Id, [], 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ShuffleTrackRowDto row = Assert.Single(result.Value);
        Assert.Null(row.CoverPath);
    }

    [Fact]
    public async Task GetShuffleTracksLiteAsync_WhenTheAlbumOnlyHasANonCoverArtwork_ShouldNotUseItAsTheCover()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-shuffle-noncover-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        await SeedShuffleTracksAsync(context, album, libraryId, 1);
        await AddArtworkAsync(context, album.Id, ArtworkType.Booklet, "booklet-page.jpg");

        // Act
        Result<IReadOnlyList<ShuffleTrackRowDto>> result = await sut.GetShuffleTracksLiteAsync(
            libraryId, null, album.Id, [], 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ShuffleTrackRowDto row = Assert.Single(result.Value);
        Assert.Null(row.CoverPath);
    }

    [Fact]
    public async Task GetShuffleTracksLiteAsync_WhenOnlyAnotherAlbumHasACover_ShouldNotUseThatCover()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-shuffle-othercover-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        await SeedShuffleTracksAsync(context, album, libraryId, 1);
        AlbumEntity otherAlbum = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "News of the World");
        await AddArtworkAsync(context, otherAlbum.Id, ArtworkType.Cover, "news-of-the-world-cover.jpg");

        // Act
        Result<IReadOnlyList<ShuffleTrackRowDto>> result = await sut.GetShuffleTracksLiteAsync(
            libraryId, null, album.Id, [], 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ShuffleTrackRowDto row = Assert.Single(result.Value);
        Assert.Null(row.CoverPath);
    }

    [Fact]
    public async Task InsertAsync_WhenATrackWithTheSamePathIsSoftDeleted_ShouldReturnTrackAlreadyExists()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-insert-deleted-path-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity deletedTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/deleted.flac", includeMetadata: false);
        deletedTrack.IsDeleted = true;
        deletedTrack.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        context.Tracks.Add(deletedTrack);
        await context.SaveChangesAsync();

        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/deleted.flac", includeMetadata: false);

        // Act
        Result<Created> result = await sut.InsertAsync(track, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task GetExistingPathsAsync_WhenTheOnlyTrackIsSoftDeleted_ShouldStillReportThePath()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-existing-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity deletedTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/deleted.flac", includeMetadata: false);
        deletedTrack.IsDeleted = true;
        deletedTrack.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        context.Tracks.Add(deletedTrack);
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyCollection<string>> result = await sut.GetExistingPathsAsync(libraryId, ["/music/queen/deleted.flac"], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(["/music/queen/deleted.flac"], result.Value);
    }

    [Fact]
    public async Task GetByPathAsync_WhenTheTrackIsSoftDeleted_ShouldReturnIt()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getbypath-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity deletedTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/deleted.flac", includeMetadata: false);
        deletedTrack.IsDeleted = true;
        deletedTrack.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        context.Tracks.Add(deletedTrack);
        await context.SaveChangesAsync();

        // Act
        Result<TrackEntity?> result = await sut.GetByPathAsync(libraryId, "/music/queen/deleted.flac", CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(deletedTrack.Id, result.Value!.Id);
        Assert.True(result.Value.IsDeleted);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTheTrackIsSoftDeleted_ShouldReturnNull()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getbyid-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        track.IsDeleted = true;
        track.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        context.Tracks.Add(track);
        await context.SaveChangesAsync();

        // Act
        Result<TrackEntity?> result = await sut.GetByIdAsync(track.Id, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Theory]
    [InlineData(true)] // The album of the track is soft deleted.
    [InlineData(false)] // The artist of the album is soft deleted.
    public async Task GetByIdAsync_WhenAnAncestorIsSoftDeleted_ShouldReturnNull(bool isAlbumDeleted)
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getbyid-deleted-ancestor-{isAlbumDeleted}-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        context.Tracks.Add(track);
        if (isAlbumDeleted)
            album.IsDeleted = true;
        else
            artist.IsDeleted = true;
        await context.SaveChangesAsync();

        // Act
        Result<TrackEntity?> result = await sut.GetByIdAsync(track.Id, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetByAlbumIdAsync_WhenTracksAreSoftDeleted_ShouldExcludeThem()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getbyalbum-deleted-tracks-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity activeTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        TrackEntity deletedTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        deletedTrack.IsDeleted = true;
        deletedTrack.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        context.Tracks.AddRange(activeTrack, deletedTrack);
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<TrackEntity>> result = await sut.GetByAlbumIdAsync(album.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(activeTrack.Id, Assert.Single(result.Value).Id);
    }

    [Fact]
    public async Task GetByAlbumIdAsync_WhenTheAlbumIsSoftDeleted_ShouldReturnEmpty()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getbyalbum-deleted-album-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        context.Tracks.Add(track);
        album.IsDeleted = true;
        album.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<TrackEntity>> result = await sut.GetByAlbumIdAsync(album.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetByAlbumIdIncludingDeletedAsync_WhenCalled_ShouldReturnEveryTrackOfTheAlbumOrdered()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getbyalbum-including-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity activeTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, trackNumber: 2, includeMetadata: false);
        activeTrack.DiscNumber = 1;
        TrackEntity deletedTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, trackNumber: 1, includeMetadata: false);
        deletedTrack.DiscNumber = 1;
        deletedTrack.IsDeleted = true;
        deletedTrack.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        context.Tracks.AddRange(activeTrack, deletedTrack);
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<TrackEntity>> result = await sut.GetByAlbumIdIncludingDeletedAsync(album.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal([deletedTrack.Id, activeTrack.Id], result.Value.Select(track => track.Id));
    }

    [Fact]
    public async Task GetShuffleTracksLiteAsync_WhenTracksOrAncestorsAreSoftDeleted_ShouldExcludeThem()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-shuffle-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity visibleArtist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity visibleAlbum = await SeedAlbumForArtistAsync(context, libraryId, visibleArtist.Id, "A Night at the Opera");
        TrackEntity visibleTrack = _trackEntityFixture.Create(albumId: visibleAlbum.Id, libraryId: libraryId, includeMetadata: false);
        TrackEntity deletedTrack = _trackEntityFixture.Create(albumId: visibleAlbum.Id, libraryId: libraryId, includeMetadata: false);
        deletedTrack.IsDeleted = true;
        deletedTrack.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        context.Tracks.AddRange(visibleTrack, deletedTrack);

        ArtistEntity artistOfDeletedAlbum = await SeedArtistAsync(context, libraryId, "Deleted Album Artist");
        AlbumEntity deletedAlbum = await SeedAlbumForArtistAsync(context, libraryId, artistOfDeletedAlbum.Id, "Deleted Album");
        context.Tracks.Add(_trackEntityFixture.Create(albumId: deletedAlbum.Id, libraryId: libraryId, includeMetadata: false));
        deletedAlbum.IsDeleted = true;
        deletedAlbum.DeletedOnUtc = _mockDateTimeProvider.UtcNow;

        ArtistEntity deletedArtist = await SeedArtistAsync(context, libraryId, "Deleted Artist");
        AlbumEntity albumOfDeletedArtist = await SeedAlbumForArtistAsync(context, libraryId, deletedArtist.Id, "Hidden Album");
        context.Tracks.Add(_trackEntityFixture.Create(albumId: albumOfDeletedArtist.Id, libraryId: libraryId, includeMetadata: false));
        deletedArtist.IsDeleted = true;
        deletedArtist.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<ShuffleTrackRowDto>> result = await sut.GetShuffleTracksLiteAsync(libraryId, null, null, [], 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(visibleTrack.Id, Assert.Single(result.Value).TrackId);
    }

    [Theory]
    [InlineData(true)] // The track itself is soft deleted.
    [InlineData(false)] // An ancestor of the track is soft deleted.
    public async Task GetTrackStreamInfoAsync_WhenTheTrackIsOrHasASoftDeletedAncestor_ShouldReturnNull(bool isTrackDeleted)
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-stream-deleted-{isTrackDeleted}-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        context.Tracks.Add(track);
        if (isTrackDeleted)
            track.IsDeleted = true;
        else
            album.IsDeleted = true;
        await context.SaveChangesAsync();

        // Act
        Result<TrackStreamDto?> result = await sut.GetTrackStreamInfoAsync(libraryId, album.Id, artist.Id, track.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Theory]
    [InlineData(true)] // The track itself is soft deleted.
    [InlineData(false)] // An ancestor of the track is soft deleted.
    public async Task GetPlaybackManifestInfoAsync_WhenTheTrackIsOrHasASoftDeletedAncestor_ShouldReturnNull(bool isTrackDeleted)
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-manifest-deleted-{isTrackDeleted}-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        context.Tracks.Add(track);
        if (isTrackDeleted)
            track.IsDeleted = true;
        else
            artist.IsDeleted = true;
        await context.SaveChangesAsync();

        // Act
        Result<TrackPlaybackManifestDto?> result = await sut.GetPlaybackManifestInfoAsync(libraryId, album.Id, artist.Id, track.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetAccessInfoByIdsAsync_WhenTracksOrAncestorsAreSoftDeleted_ShouldExcludeThem()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-access-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        TrackEntity activeTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        TrackEntity deletedTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        deletedTrack.IsDeleted = true;
        deletedTrack.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        context.Tracks.AddRange(activeTrack, deletedTrack);

        ArtistEntity deletedArtist = await SeedArtistAsync(context, libraryId, "Deleted Artist");
        AlbumEntity albumOfDeletedArtist = await SeedAlbumForArtistAsync(context, libraryId, deletedArtist.Id, "Hidden Album");
        TrackEntity trackOfDeletedArtist = _trackEntityFixture.Create(albumId: albumOfDeletedArtist.Id, libraryId: libraryId, includeMetadata: false);
        context.Tracks.Add(trackOfDeletedArtist);
        deletedArtist.IsDeleted = true;
        deletedArtist.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        await context.SaveChangesAsync();

        // Act
        Result<TrackAccessSummaryDto> result = await sut.GetAccessInfoByIdsAsync([activeTrack.Id, deletedTrack.Id, trackOfDeletedArtist.Id], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(1, result.Value.TrackCount);
        Assert.Equal([libraryId], result.Value.LibraryIds);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenTracksOrAncestorsAreSoftDeleted_ShouldExcludeThem()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getbyids-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        TrackEntity activeTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        TrackEntity deletedTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        deletedTrack.IsDeleted = true;
        deletedTrack.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        context.Tracks.AddRange(activeTrack, deletedTrack);

        ArtistEntity artistOfDeletedAlbum = await SeedArtistAsync(context, libraryId, "Album Artist");
        AlbumEntity deletedAlbum = await SeedAlbumForArtistAsync(context, libraryId, artistOfDeletedAlbum.Id, "Deleted Album");
        TrackEntity trackOfDeletedAlbum = _trackEntityFixture.Create(albumId: deletedAlbum.Id, libraryId: libraryId, includeMetadata: false);
        context.Tracks.Add(trackOfDeletedAlbum);
        deletedAlbum.IsDeleted = true;
        deletedAlbum.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<TrackEntity>> result = await sut.GetByIdsAsync([activeTrack.Id, deletedTrack.Id, trackOfDeletedAlbum.Id], false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(activeTrack.Id, Assert.Single(result.Value).Id);
    }

    [Fact]
    public async Task GetTracksNeedingMetadataCountAsync_WhenATrackOrItsAncestorsAreSoftDeleted_ShouldNotCountIt()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-count-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumForArtistAsync(context, libraryId, artist.Id, "A Night at the Opera");
        TrackEntity pendingTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false, metadataStatus: MetadataStatus.Pending);
        TrackEntity deletedTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false, metadataStatus: MetadataStatus.Pending);
        deletedTrack.IsDeleted = true;
        deletedTrack.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        context.Tracks.AddRange(pendingTrack, deletedTrack);

        ArtistEntity artistOfDeletedAlbum = await SeedArtistAsync(context, libraryId, "Album Artist");
        AlbumEntity deletedAlbum = await SeedAlbumForArtistAsync(context, libraryId, artistOfDeletedAlbum.Id, "Deleted Album");
        context.Tracks.Add(_trackEntityFixture.Create(
            albumId: deletedAlbum.Id, libraryId: libraryId, includeMetadata: false, metadataStatus: MetadataStatus.Pending));
        deletedAlbum.IsDeleted = true;
        deletedAlbum.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        await context.SaveChangesAsync();

        // Act
        Result<int> result = await sut.GetTracksNeedingMetadataCountAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(1, result.Value);
    }

    [Fact]
    public async Task SoftDeleteByIdAsync_WhenCalled_ShouldPersistTheDeletionStateAndProviderTime()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-softdelete-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        context.Tracks.Add(track);
        await context.SaveChangesAsync();

        // Act
        Result<Updated> result = await sut.SoftDeleteByIdAsync(track.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity storedTrack = await context.Tracks.AsNoTracking().SingleAsync(candidate => candidate.Id == track.Id);
        Assert.True(storedTrack.IsDeleted);
        Assert.Equal(_mockDateTimeProvider.UtcNow, storedTrack.DeletedOnUtc);
    }

    [Fact]
    public async Task SoftDeleteByIdAsync_WhenTrackIsAlreadyDeleted_ShouldReturnTrackNotFound()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-softdelete-already-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        DateTime storedDeletedOnUtc = _mockDateTimeProvider.UtcNow.AddHours(-1);
        track.IsDeleted = true;
        track.DeletedOnUtc = storedDeletedOnUtc;
        context.Tracks.Add(track);
        await context.SaveChangesAsync();

        // Act
        Result<Updated> result = await sut.SoftDeleteByIdAsync(track.Id, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
        TrackEntity storedTrack = await context.Tracks.AsNoTracking().SingleAsync(candidate => candidate.Id == track.Id);
        Assert.Equal(storedDeletedOnUtc, storedTrack.DeletedOnUtc);
    }

    [Fact]
    public async Task SoftDeleteByIdAsync_WhenTrackDoesNotExist_ShouldReturnTrackNotFound()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-softdelete-missing-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);

        // Act
        Result<Updated> result = await sut.SoftDeleteByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
    }

    [Fact]
    public async Task RestoreSoftDeletedByIdAsync_WhenCalled_ShouldPersistTheClearedDeletionState()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-restore-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        track.IsDeleted = true;
        track.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        context.Tracks.Add(track);
        await context.SaveChangesAsync();

        // Act
        Result<Updated> result = await sut.RestoreSoftDeletedByIdAsync(track.Id, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity storedTrack = await context.Tracks.AsNoTracking().SingleAsync(candidate => candidate.Id == track.Id);
        Assert.False(storedTrack.IsDeleted);
        Assert.Null(storedTrack.DeletedOnUtc);
    }

    [Fact]
    public async Task GetSoftDeletedByLibraryIdAsync_WhenCalled_ShouldReturnOnlyDeletedTracksWithVisibleAncestorsOrdered()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-getdeleted-library-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        DateTime deletedOnUtc = _mockDateTimeProvider.UtcNow;
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity olderDeletion = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, title: "Alpha", includeMetadata: false);
        olderDeletion.IsDeleted = true;
        olderDeletion.DeletedOnUtc = deletedOnUtc.AddHours(-2);
        TrackEntity newerDeletion = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, title: "Beta", includeMetadata: false);
        newerDeletion.IsDeleted = true;
        newerDeletion.DeletedOnUtc = deletedOnUtc.AddHours(-1);
        TrackEntity sameTimeDeletion = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, title: "Gamma", includeMetadata: false);
        sameTimeDeletion.IsDeleted = true;
        sameTimeDeletion.DeletedOnUtc = deletedOnUtc.AddHours(-1);
        context.Tracks.AddRange(olderDeletion, newerDeletion, sameTimeDeletion);

        ArtistEntity deletedArtist = await SeedArtistAsync(context, libraryId, "Deleted Artist");
        AlbumEntity albumOfDeletedArtist = await SeedAlbumForArtistAsync(context, libraryId, deletedArtist.Id, "Hidden Album");
        TrackEntity hiddenDeletedTrack = _trackEntityFixture.Create(albumId: albumOfDeletedArtist.Id, libraryId: libraryId, includeMetadata: false);
        hiddenDeletedTrack.IsDeleted = true;
        hiddenDeletedTrack.DeletedOnUtc = deletedOnUtc;
        context.Tracks.Add(hiddenDeletedTrack);
        deletedArtist.IsDeleted = true;
        deletedArtist.DeletedOnUtc = deletedOnUtc;
        await context.SaveChangesAsync();

        TrackEntity[] expectedDeletedTracks = [olderDeletion, newerDeletion, sameTimeDeletion];
        Guid[] expectedOrder = [.. expectedDeletedTracks
            .OrderByDescending(track => track.DeletedOnUtc)
            .ThenByDescending(track => track.Title)
            .ThenByDescending(track => track.Id)
            .Select(track => track.Id)];

        // Act
        Result<IReadOnlyList<TrackEntity>> result = await sut.GetSoftDeletedByLibraryIdAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedOrder, result.Value.Select(track => track.Id));
    }

    [Fact]
    public async Task UpdateAsync_WhenTheTrackIsSoftDeleted_ShouldReturnTrackNotFound()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-update-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        track.IsDeleted = true;
        track.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        context.Tracks.Add(track);
        await context.SaveChangesAsync();

        TrackEntity data = _trackEntityFixture.Create(id: track.Id, albumId: album.Id, libraryId: libraryId, includeMetadata: false);

        // Act
        Result<Updated> result = await sut.UpdateAsync(data, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
    }

    [Fact]
    public async Task UpdateAsync_WhenIncomingDataIsMarkedDeleted_ShouldPreserveTheActiveRowsDeletionState()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-trackrepo-update-preserve-deletion-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = CreateContext(anchorConnection);
        TrackRepository sut = CreateTrackRepository(context);
        Guid libraryId = Guid.NewGuid();
        AlbumEntity album = await SeedAlbumAsync(context, libraryId);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        context.Tracks.Add(track);
        await context.SaveChangesAsync();

        TrackEntity data = await LoadDetachedTrackAsync(context, track.Id);
        data.Title = "Somebody to Love";
        data.IsDeleted = true;
        data.DeletedOnUtc = _mockDateTimeProvider.UtcNow;

        // Act
        Result<Updated> result = await sut.UpdateAsync(data, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        TrackEntity storedTrack = await context.Tracks.AsNoTracking().SingleAsync(candidate => candidate.Id == track.Id);
        Assert.Equal("Somebody to Love", storedTrack.Title);
        Assert.False(storedTrack.IsDeleted);
        Assert.Null(storedTrack.DeletedOnUtc);
    }

    /// <summary>
    /// Creates a list of tracks of the provided album with distinct paths and empty metadata collections, so that the chunked lookup can be exercised without extra graph data.
    /// </summary>
    /// <param name="count">The number of tracks to create.</param>
    /// <param name="albumId">The Id of the album that owns the tracks.</param>
    /// <param name="libraryId">The Id of the library that owns the tracks.</param>
    /// <returns>The created tracks.</returns>
    private List<TrackEntity> CreateTracks(int count, Guid albumId, Guid libraryId)
    {
        return [.. Enumerable.Range(0, count).Select(_ => _trackEntityFixture.Create(
            albumId: albumId, libraryId: libraryId, path: $"/music/generated-{Guid.NewGuid():N}.flac", includeMetadata: false))];
    }

    /// <summary>
    /// Creates a track repository backed by the provided context, using the deterministic time provider of the test class.
    /// </summary>
    /// <param name="context">The context the repository operates on.</param>
    /// <returns>The created repository.</returns>
    private TrackRepository CreateTrackRepository(NemeraDbContext context)
    {
        return new TrackRepository(context, _mockDateTimeProvider);
    }

    /// <summary>
    /// Creates a real SQLite backed context, so that the stored values are read exactly as they were written.
    /// </summary>
    /// <param name="anchorConnection">The open in-memory SQLite connection that keeps the database alive for the duration of the test.</param>
    /// <returns>The created context.</returns>
    private static NemeraDbContext CreateContext(SqliteConnection anchorConnection)
    {
        NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>()
            .UseSqlite(anchorConnection.ConnectionString)
            .Options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Seeds an artist and the album that owns the tracks of the library identified by <paramref name="libraryId"/>, so that the required foreign keys of a track are satisfied.
    /// </summary>
    /// <param name="context">The context the artist and album are persisted through.</param>
    /// <param name="libraryId">The Id of the library the album belongs to.</param>
    /// <param name="includeMetadata">Whether the album and its artist should carry tags and genres, or empty collections.</param>
    /// <returns>The persisted album.</returns>
    private async Task<AlbumEntity> SeedAlbumAsync(NemeraDbContext context, Guid libraryId, bool includeMetadata = false)
    {
        ArtistEntity? artist = await context.Artists.FirstOrDefaultAsync(existing => existing.LibraryId == libraryId);
        if (artist is null)
        {
            artist = _artistEntityFixture.Create(
                libraryId: libraryId, name: "Queen", includeAlbums: false, includeContributors: false);
            if (includeMetadata)
            {
                artist.Tags = [_tagEntityFixture.Create(name: $"ArtistTag-{Guid.NewGuid():N}")];
                artist.Genres = [_genreEntityFixture.Create(name: $"ArtistGenre-{Guid.NewGuid():N}")];
            }
            context.Artists.Add(artist);
            await context.SaveChangesAsync();
        }
        AlbumEntity album = _albumEntityFixture.Create(
            artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        if (includeMetadata)
        {
            album.Tags = [_tagEntityFixture.Create(name: $"AlbumTag-{Guid.NewGuid():N}")];
            album.Genres = [_genreEntityFixture.Create(name: $"AlbumGenre-{Guid.NewGuid():N}")];
        }
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        return album;
    }

    /// <summary>
    /// Creates a real SQLite backed context with the auditing interceptor attached, so that audit columns behave exactly like in production.
    /// </summary>
    /// <param name="anchorConnection">The open in-memory SQLite connection that keeps the database alive for the duration of the test.</param>
    /// <param name="userId">The Id of the user reported as the current user.</param>
    /// <param name="utcNow">The current UTC time reported by the time provider.</param>
    /// <returns>The created context.</returns>
    private static NemeraDbContext CreateAuditedContext(SqliteConnection anchorConnection, Guid userId, DateTime utcNow)
    {
        ICurrentUserService currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(userId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(utcNow);
        NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>()
            .UseSqlite(anchorConnection.ConnectionString)
            .AddInterceptors(new UpdateAuditableEntitiesInterceptor(currentUserService, dateTimeProvider))
            .Options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Loads a detached copy of a stored track, with its children, so that it can be handed to the repository as the desired state of an edit.
    /// </summary>
    /// <param name="context">The context that tracks the stored track.</param>
    /// <param name="trackId">The Id of the track to load.</param>
    /// <returns>The detached copy of the track.</returns>
    private static async Task<TrackEntity> LoadDetachedTrackAsync(NemeraDbContext context, Guid trackId)
    {
        return await context.Tracks
            .AsNoTracking()
            .Include(track => track.Tags)
            .Include(track => track.Genres)
            .Include(track => track.Ratings)
            .Include(track => track.Contributors)
            .Include(track => track.Moods)
            .Include(track => track.Isrcs)
            .FirstAsync(track => track.Id == trackId);
    }

    /// <summary>
    /// Seeds an artist of the library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="context">The context the artist is persisted through.</param>
    /// <param name="libraryId">The Id of the library the artist belongs to.</param>
    /// <param name="name">The name of the artist.</param>
    /// <returns>The persisted artist.</returns>
    private async Task<ArtistEntity> SeedArtistAsync(NemeraDbContext context, Guid libraryId, string name)
    {
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, name: name, includeAlbums: false, includeContributors: false);
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        return artist;
    }

    /// <summary>
    /// Seeds an album of the artist identified by <paramref name="artistId"/>.
    /// </summary>
    /// <param name="context">The context the album is persisted through.</param>
    /// <param name="libraryId">The Id of the library the album belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="title">The title of the album.</param>
    /// <returns>The persisted album.</returns>
    private async Task<AlbumEntity> SeedAlbumForArtistAsync(NemeraDbContext context, Guid libraryId, Guid artistId, string title)
    {
        AlbumEntity album = _albumEntityFixture.Create(
            artistId: artistId, libraryId: libraryId, title: title, includeTracks: false, includeMetadata: false);
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        return album;
    }

    /// <summary>
    /// Seeds a number of tracks of the album identified by <paramref name="album"/>, with distinct paths and no metadata collections.
    /// </summary>
    /// <param name="context">The context the tracks are persisted through.</param>
    /// <param name="album">The album that owns the tracks.</param>
    /// <param name="libraryId">The Id of the library that owns the tracks.</param>
    /// <param name="count">The number of tracks to seed.</param>
    /// <returns>The persisted tracks.</returns>
    private async Task<List<TrackEntity>> SeedShuffleTracksAsync(NemeraDbContext context, AlbumEntity album, Guid libraryId, int count)
    {
        List<TrackEntity> tracks = [.. Enumerable.Range(0, count).Select(index => _trackEntityFixture.Create(
            albumId: album.Id,
            libraryId: libraryId,
            path: $"/music/shuffle-{Guid.NewGuid():N}.flac",
            title: $"Shuffle Track {index}",
            includeMetadata: false))];
        context.Tracks.AddRange(tracks);
        await context.SaveChangesAsync();
        return tracks;
    }

    /// <summary>
    /// Seeds a piece of artwork of the album identified by <paramref name="albumId"/>.
    /// </summary>
    /// <param name="context">The context the artwork is persisted through.</param>
    /// <param name="albumId">The Id of the album that owns the artwork.</param>
    /// <param name="artworkType">The type of the artwork.</param>
    /// <param name="fileName">The relative file name of the stored artwork.</param>
    private async Task AddArtworkAsync(NemeraDbContext context, Guid albumId, ArtworkType artworkType, string fileName)
    {
        context.MusicArtwork.Add(_musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album,
            ownerId: albumId,
            artworkType: artworkType,
            ordinal: 0,
            fileName: fileName,
            status: ArtworkStatus.Enriched));
        await context.SaveChangesAsync();
    }
}
