#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.Repositories.MusicLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
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

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.MusicLibrary;

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
    private readonly PaginationDataDtoFixture _paginationDataDtoFixture = new();

    [Fact]
    public async Task InsertAsync_WhenCalledWithAValidTrack_ShouldPersistTheTrackAndItsChildren()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-trackrepo-insert-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = new(context);

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
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-trackrepo-insert-conflict-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = new(context);

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
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-trackrepo-insert-otherlibrary-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = new(context);

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
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-trackrepo-existingpaths-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = new(context);

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
    public async Task UpdateAsync_WhenNothingChanged_ShouldNotChangeAnyAuditColumn()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-trackrepo-update-noop-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        TrackRepository sut = new(context);

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
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-trackrepo-update-metadata-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        TrackRepository sut = new(context);

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
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-trackrepo-update-removecontributor-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        TrackRepository sut = new(context);

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
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-trackrepo-update-addcontributor-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        TrackRepository sut = new(context);

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
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-trackrepo-update-tags-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        TrackRepository sut = new(context);

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
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-trackrepo-update-owned-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        TrackRepository sut = new(context);

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
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-trackrepo-getbyid-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = new(context);

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
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-trackrepo-getbyid-notracking-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = new(context);

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
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-trackrepo-getbyalbum-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = new(context);

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
            .OrderBy(track => track.DiscNumber)
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
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-trackrepo-lite-null-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = new(context);

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
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-trackrepo-lite-page-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        TrackRepository sut = new(context);

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

    /// <summary>
    /// Seeds an artist and the album that owns the tracks of the library identified by <paramref name="libraryId"/>, so that the required foreign keys of a track are satisfied.
    /// </summary>
    /// <param name="context">The context the artist and album are persisted through.</param>
    /// <param name="libraryId">The Id of the library the album belongs to.</param>
    /// <returns>The persisted album.</returns>
    private async Task<AlbumEntity> SeedAlbumAsync(LuminaDbContext context, Guid libraryId)
    {
        ArtistEntity? artist = await context.Artists.FirstOrDefaultAsync(existing => existing.LibraryId == libraryId);
        if (artist is null)
        {
            artist = _artistEntityFixture.Create(
                libraryId: libraryId, name: "Queen", includeAlbums: false, includeContributors: false);
            context.Artists.Add(artist);
            await context.SaveChangesAsync();
        }
        AlbumEntity album = _albumEntityFixture.Create(
            artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
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
    private static LuminaDbContext CreateAuditedContext(SqliteConnection anchorConnection, Guid userId, DateTime utcNow)
    {
        ICurrentUserService currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(userId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(utcNow);
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>()
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
    private static async Task<TrackEntity> LoadDetachedTrackAsync(LuminaDbContext context, Guid trackId)
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
}
