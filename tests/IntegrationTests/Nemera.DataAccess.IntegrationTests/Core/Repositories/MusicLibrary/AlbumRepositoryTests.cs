#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Common;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Nemera.Application.Common.DTO.Pagination;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Time;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.Common;
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
/// Contains integration tests for the <see cref="AlbumRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumRepositoryTests
{
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
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();
    private readonly PaginationDataDtoFixture _paginationDataDtoFixture = new();
    private readonly IDateTimeProvider _mockDateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlbumRepositoryTests"/> class.
    /// </summary>
    public AlbumRepositoryTests()
    {
        _mockDateTimeProvider = Substitute.For<IDateTimeProvider>();
        _mockDateTimeProvider.UtcNow.Returns(new DateTime(2026, 11, 14, 10, 30, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task InsertAsync_WhenCalledWithAValidAlbum_ShouldPersistTheAlbumAndItsAggregate()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-insert-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = BuildFullAlbum(artist.Id, libraryId, "A Night at the Opera");

        // Act
        Result<Created> result = await sut.InsertAsync(album, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        AlbumEntity? storedAlbum = await context.Albums
            .AsNoTracking()
            .Include(candidate => candidate.Ratings)
            .Include(candidate => candidate.Tags)
            .Include(candidate => candidate.Genres)
            .Include(candidate => candidate.Contributors)
            .Include(candidate => candidate.Tracks)
                .ThenInclude(candidate => candidate.Moods)
            .AsSplitQuery()
            .FirstOrDefaultAsync(candidate => candidate.Id == album.Id);
        Assert.NotNull(storedAlbum);
        Assert.Equal("A Night at the Opera", storedAlbum!.Title);
        Assert.Single(storedAlbum.Ratings);
        Assert.Equal(2, storedAlbum.Tags.Count);
        Assert.Equal(2, storedAlbum.Genres.Count);
        Assert.Single(storedAlbum.Contributors);
        TrackEntity storedTrack = Assert.Single(storedAlbum.Tracks);
        Assert.Equal(2, storedTrack.Moods.Count);
        // Each tag and genre of the aggregate is stored exactly once.
        Assert.Single(await context.Set<TagEntity>().Where(tag => tag.Name == "album-tag-1").ToListAsync());
        Assert.Single(await context.Set<GenreEntity>().Where(genre => genre.Name == "album-genre-1").ToListAsync());
    }

    [Fact]
    public async Task InsertAsync_WhenAlbumWithTheSameIdAlreadyExists_ShouldReturnAlbumAlreadyExists()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-insert-id-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity existingAlbum = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        context.Albums.Add(existingAlbum);
        await context.SaveChangesAsync();

        AlbumEntity album = _albumEntityFixture.Create(id: existingAlbum.Id, artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);

        // Act
        Result<Created> result = await sut.InsertAsync(album, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task InsertAsync_WhenTwoTracksOfTheSameRequestShareAPath_ShouldReturnTrackAlreadyExists()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-insert-duprequest-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        AlbumEntity album = _albumEntityFixture.Create(includeTracks: false, includeMetadata: false);
        album.Tracks =
        [
            _trackEntityFixture.Create(albumId: album.Id, libraryId: album.LibraryId, path: "/music/queen/duplicate.flac", includeMetadata: false),
            _trackEntityFixture.Create(albumId: album.Id, libraryId: album.LibraryId, path: "/music/queen/duplicate.flac", includeMetadata: false)
        ];

        // Act
        Result<Created> result = await sut.InsertAsync(album, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task InsertAsync_WhenATrackPathIsAlreadyStoredInTheLibrary_ShouldReturnTrackAlreadyExists()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-insert-storedpath-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity existingAlbum = await SeedAlbumAsync(context, libraryId, artist.Id, "Existing");
        context.Tracks.Add(_trackEntityFixture.Create(
            albumId: existingAlbum.Id, libraryId: libraryId, path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false));
        await context.SaveChangesAsync();

        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Tracks = [_trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false)];

        // Act
        Result<Created> result = await sut.InsertAsync(album, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task InsertAsync_WhenAnotherLibraryHasATrackWithTheSamePath_ShouldPersistTheAlbum()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-insert-otherlibrary-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid firstLibraryId = Guid.NewGuid();
        ArtistEntity firstArtist = await SeedArtistAsync(context, firstLibraryId);
        AlbumEntity firstAlbum = await SeedAlbumAsync(context, firstLibraryId, firstArtist.Id, "First");
        context.Tracks.Add(_trackEntityFixture.Create(
            albumId: firstAlbum.Id, libraryId: firstLibraryId, path: "/music/queen/we-are-the-champions.flac", includeMetadata: false));
        await context.SaveChangesAsync();

        Guid secondLibraryId = Guid.NewGuid();
        ArtistEntity secondArtist = await SeedArtistAsync(context, secondLibraryId);
        AlbumEntity album = _albumEntityFixture.Create(artistId: secondArtist.Id, libraryId: secondLibraryId, includeTracks: false, includeMetadata: false);
        album.Tracks = [_trackEntityFixture.Create(albumId: album.Id, libraryId: secondLibraryId, path: "/music/queen/we-are-the-champions.flac", includeMetadata: false)];

        // Act
        Result<Created> result = await sut.InsertAsync(album, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        AlbumEntity? storedAlbum = await context.Albums.AsNoTracking().FirstOrDefaultAsync(candidate => candidate.Id == album.Id);
        Assert.NotNull(storedAlbum);
        Assert.Equal(secondLibraryId, storedAlbum!.LibraryId);
    }

    [Fact]
    public async Task InsertAsync_WhenTagsAndGenresAlreadyExistByName_ShouldReuseTheStoredRows()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-insert-reuse-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        context.Set<TagEntity>().Add(_tagEntityFixture.Create(name: "ExistingTag"));
        context.Set<GenreEntity>().Add(_genreEntityFixture.Create(name: "ExistingGenre"));
        await context.SaveChangesAsync();

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Tags = [_tagEntityFixture.Create(name: "ExistingTag")];
        album.Genres = [_genreEntityFixture.Create(name: "ExistingGenre")];
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: album.LibraryId, includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "ExistingTag")];
        track.Genres = [_genreEntityFixture.Create(name: "ExistingGenre")];
        album.Tracks = [track];

        // Act
        Result<Created> result = await sut.InsertAsync(album, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        // The shared rows are reused instead of being duplicated.
        Assert.Single(await context.Set<TagEntity>().Where(tag => tag.Name == "ExistingTag").ToListAsync());
        Assert.Single(await context.Set<GenreEntity>().Where(genre => genre.Name == "ExistingGenre").ToListAsync());
    }

    [Fact]
    public async Task InsertAsync_WhenANewTagAndGenreNameIsSharedBetweenTheAlbumAndItsTracks_ShouldPersistItOnce()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-insert-shared-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        album.Tags = [_tagEntityFixture.Create(name: "SharedNewTag")];
        album.Genres = [_genreEntityFixture.Create(name: "SharedNewGenre")];
        track.Tags = [_tagEntityFixture.Create(name: "SharedNewTag")];
        track.Genres = [_genreEntityFixture.Create(name: "SharedNewGenre")];
        album.Tracks = [track];

        // Act
        Result<Created> result = await sut.InsertAsync(album, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        // The name shared by the album and its tracks is persisted exactly once.
        Assert.Single(await context.Set<TagEntity>().Where(tag => tag.Name == "SharedNewTag").ToListAsync());
        Assert.Single(await context.Set<GenreEntity>().Where(genre => genre.Name == "SharedNewGenre").ToListAsync());
    }

    [Fact]
    public async Task UpdateAsync_WhenNothingChanged_ShouldNotChangeAnyAuditColumn()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-update-noop-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using NemeraDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = await SeedFullAlbumAsync(context, libraryId, artist.Id, "A Night at the Opera");
        Guid contributorId = album.Contributors[0].Id;
        DateTime contributorCreatedOnUtc = album.Contributors[0].CreatedOnUtc;
        // Detach the seeded graph, so that the update must load the tracked album through its include chain instead of returning the already populated seeded instance.
        context.ChangeTracker.Clear();
        AlbumEntity incoming = await LoadDetachedAlbumAsync(context, album.Id);

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        AlbumEntity? storedAlbum = await context.Albums.AsNoTracking().Include(candidate => candidate.Contributors).FirstOrDefaultAsync(candidate => candidate.Id == album.Id);
        Assert.NotNull(storedAlbum);
        Assert.Null(storedAlbum!.UpdatedOnUtc);
        AlbumContributorEntity storedContributor = Assert.Single(storedAlbum.Contributors);
        Assert.Equal(contributorId, storedContributor.Id);
        Assert.Equal(contributorCreatedOnUtc, storedContributor.CreatedOnUtc);
        Assert.Single(await context.Set<TagEntity>().Where(tag => tag.Name == "album-tag-1").ToListAsync());
        Assert.Single(await context.Set<GenreEntity>().Where(genre => genre.Name == "album-genre-1").ToListAsync());
    }

    [Fact]
    public async Task UpdateAsync_WhenOnlyTheScalarValuesChanged_ShouldStampTheAlbumAndPreserveArtistAndLibrary()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-update-scalars-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using NemeraDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = await SeedFullAlbumAsync(context, libraryId, artist.Id, "A Night at the Opera");
        AlbumEntity incoming = await LoadDetachedAlbumAsync(context, album.Id);
        incoming.Title = "A Night at the Opera (Remastered)";
        incoming.ArtistId = Guid.NewGuid();
        incoming.LibraryId = Guid.NewGuid();

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        AlbumEntity? storedAlbum = await context.Albums.AsNoTracking().FirstOrDefaultAsync(candidate => candidate.Id == album.Id);
        Assert.NotNull(storedAlbum);
        Assert.Equal("A Night at the Opera (Remastered)", storedAlbum!.Title);
        // The stored identity is preserved and only the actual edit is stamped.
        Assert.Equal(artist.Id, storedAlbum.ArtistId);
        Assert.Equal(libraryId, storedAlbum.LibraryId);
        Assert.Equal(utcNow, storedAlbum.UpdatedOnUtc);
        Assert.Equal(userId, storedAlbum.UpdatedBy);
    }

    [Fact]
    public async Task UpdateAsync_WhenRatingsAndContributorsChange_ShouldReconcileThem()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-update-owned-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        Guid keptMediaContributorId = Guid.NewGuid();
        Guid removedMediaContributorId = Guid.NewGuid();
        Guid addedMediaContributorId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Ratings =
        [
            _audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100),
            _audioRatingEntityFixture.Create(value: 5, maxValue: 5, source: AudioRatingSource.User, voteCount: 10),
            _audioRatingEntityFixture.Create(value: 2, maxValue: 5, source: AudioRatingSource.LastFm, voteCount: 20)
        ];
        album.Contributors =
        [
            _albumContributorEntityFixture.Create(albumId: album.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Producer),
            _albumContributorEntityFixture.Create(albumId: album.Id, mediaContributorId: removedMediaContributorId, role: MediaContributorRole.Composer)
        ];
        context.Albums.Add(album);
        await context.SaveChangesAsync();

        AlbumEntity incoming = await LoadDetachedAlbumAsync(context, album.Id);
        incoming.Ratings =
        [
            _audioRatingEntityFixture.Create(value: 3, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100),
            _audioRatingEntityFixture.Create(value: 5, maxValue: 5, source: AudioRatingSource.User, voteCount: 10),
            _audioRatingEntityFixture.Create(value: 1, maxValue: 5, source: AudioRatingSource.Discogs, voteCount: 5)
        ];
        incoming.Contributors =
        [
            _albumContributorEntityFixture.Create(albumId: album.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Producer),
            _albumContributorEntityFixture.Create(albumId: album.Id, mediaContributorId: addedMediaContributorId, role: MediaContributorRole.Vocals)
        ];

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        AlbumEntity? storedAlbum = await context.Albums
            .AsNoTracking()
            .Include(candidate => candidate.Ratings)
            .Include(candidate => candidate.Contributors)
            .FirstOrDefaultAsync(candidate => candidate.Id == album.Id);
        Assert.NotNull(storedAlbum);
        Assert.Equal(3, storedAlbum!.Ratings.Count);
        Assert.Contains(storedAlbum.Ratings, rating => rating.Source == AudioRatingSource.MusicBrainz && rating.Value == 3M);
        Assert.Contains(storedAlbum.Ratings, rating => rating.Source == AudioRatingSource.User && rating.Value == 5M);
        Assert.Contains(storedAlbum.Ratings, rating => rating.Source == AudioRatingSource.Discogs && rating.Value == 1M);
        Assert.DoesNotContain(storedAlbum.Ratings, rating => rating.Source == AudioRatingSource.LastFm);
        Assert.Equal(2, storedAlbum.Contributors.Count);
        Assert.DoesNotContain(storedAlbum.Contributors, contributor => contributor.MediaContributorId == removedMediaContributorId);
        Assert.Contains(storedAlbum.Contributors, contributor => contributor.MediaContributorId == addedMediaContributorId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAlbumExists_ShouldIncludeTheNavigationProperties()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-getbyid-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = await SeedFullAlbumAsync(context, libraryId, artist.Id, "A Night at the Opera");

        // Act
        Result<AlbumEntity?> result = await sut.GetByIdAsync(album.Id, cancellationToken: CancellationToken.None);

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
        Assert.Equal(2, retrievedTrack.Genres.Count);
        Assert.Equal(2, retrievedTrack.Moods.Count);
        Assert.Equal(2, retrievedTrack.Isrcs.Count);
        Assert.Single(retrievedTrack.Contributors);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntitiesShouldNotBeTracked_ShouldReturnAnUntrackedAlbum()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-getbyid-notracking-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<AlbumEntity?> result = await sut.GetByIdAsync(
            album.Id, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(context.ChangeTracker.Entries<AlbumEntity>());
    }

    [Fact]
    public async Task GetAlbumsLiteByArtistIdAsync_WhenCalled_ShouldNotTrackTheEntities()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-lite-notracking-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<PaginatedResultDto<AlbumLiteRow>> result = await sut.GetAlbumsLiteByArtistIdAsync(artist.Id, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value.Data);
        Assert.Empty(context.ChangeTracker.Entries<AlbumEntity>());
    }

    [Fact]
    public async Task GetByArtistIdAsync_WhenCalled_ShouldReturnTheArtistAlbumsOrderedWithTheirCollections()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-getbyartist-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity betaAlbum = await SeedFullAlbumAsync(context, libraryId, artist.Id, "Beta");
        AlbumEntity alphaAlbum = await SeedAlbumAsync(context, libraryId, artist.Id, "Alpha");
        ArtistEntity otherArtist = await SeedArtistAsync(context, libraryId);
        AlbumEntity albumOfAnotherArtist = _albumEntityFixture.Create(
            artistId: otherArtist.Id, libraryId: libraryId, title: "Gamma", includeTracks: false, includeMetadata: false);
        context.Albums.Add(albumOfAnotherArtist);
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<AlbumEntity>> result = await sut.GetByArtistIdAsync(artist.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal([alphaAlbum.Id, betaAlbum.Id], result.Value.Select(album => album.Id));
        AlbumEntity retrievedAlbum = result.Value.Single(album => album.Id == betaAlbum.Id);
        Assert.Single(retrievedAlbum.Ratings);
        Assert.Equal(2, retrievedAlbum.Tags.Count);
        Assert.Equal(2, retrievedAlbum.Genres.Count);
        Assert.Single(retrievedAlbum.Contributors);
        TrackEntity retrievedTrack = Assert.Single(retrievedAlbum.Tracks);
        Assert.Equal(2, retrievedTrack.Moods.Count);
        Assert.Single(retrievedTrack.Contributors);
    }

    [Fact]
    public async Task GetAlbumsLiteByArtistIdAsync_WhenPaginationDataIsNull_ShouldReturnAllArtistAlbumsOrdered()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-lite-null-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity betaAlbum = await SeedAlbumAsync(context, libraryId, artist.Id, "Beta");
        AlbumEntity alphaAlbum = await SeedAlbumAsync(context, libraryId, artist.Id, "Alpha");
        ArtistEntity otherArtist = await SeedArtistAsync(context, libraryId);
        AlbumEntity albumOfAnotherArtist = _albumEntityFixture.Create(
            artistId: otherArtist.Id, libraryId: libraryId, title: "Gamma", includeTracks: false, includeMetadata: false);
        context.Albums.Add(albumOfAnotherArtist);
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<AlbumLiteRow>> result = await sut.GetAlbumsLiteByArtistIdAsync(artist.Id, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal([alphaAlbum.Id, betaAlbum.Id], result.Value.Data.Select(row => row.Id));
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(1, result.Value.CurrentPage);
        Assert.Equal(2, result.Value.PerPage);
    }

    [Theory]
    [InlineData(1, 2, 1, 2)] // The first page is fully returned.
    [InlineData(2, 2, 2, 2)] // A middle page is fully returned.
    [InlineData(3, 2, 3, 1)] // The last page is partially returned.
    [InlineData(99, 2, 3, 1)] // A page beyond the last one is clamped to the last page.
    public async Task GetAlbumsLiteByArtistIdAsync_WhenPaginationIsProvided_ShouldReturnTheRequestedPage(int requestedPage, int perPage, int expectedCurrentPage, int expectedRowCount)
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-lite-page-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        List<AlbumEntity> albums = _albumEntityFixture.CreateMany(5);
        for (int index = 0; index < albums.Count; index++)
        {
            albums[index].ArtistId = artist.Id;
            albums[index].LibraryId = libraryId;
            albums[index].Title = $"Album {index}";
            albums[index].Tracks = [];
        }
        context.Albums.AddRange(albums);
        await context.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: requestedPage, perPage: perPage);

        // Act
        Result<PaginatedResultDto<AlbumLiteRow>> result = await sut.GetAlbumsLiteByArtistIdAsync(artist.Id, paginationData, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(5, result.Value.Count);
        Assert.Equal(3, result.Value.NumberOfPages);
        Assert.Equal(expectedCurrentPage, result.Value.CurrentPage);
        Assert.Equal(perPage, result.Value.PerPage);
        Assert.Equal(expectedRowCount, result.Value.Data.Count);
    }

    [Fact]
    public async Task GetAlbumsLiteByArtistIdAsync_WhenReReleaseYearIsSet_ShouldProjectItAsReleaseYear()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-lite-releaseyear-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = _albumEntityFixture.Create(
            artistId: artist.Id, libraryId: libraryId, originalReleaseYear: 1975, reReleaseYear: 2011, includeTracks: false, includeMetadata: false);
        context.Albums.Add(album);
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<AlbumLiteRow>> result = await sut.GetAlbumsLiteByArtistIdAsync(artist.Id, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        AlbumLiteRow row = Assert.Single(result.Value.Data);
        Assert.Equal(2011, row.ReleaseYear);
    }

    [Fact]
    public async Task GetAlbumsLiteByArtistIdAsync_WhenReReleaseYearIsNull_ShouldFallBackToOriginalReleaseYear()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-lite-originalyear-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = _albumEntityFixture.Create(
            artistId: artist.Id, libraryId: libraryId, originalReleaseYear: 1975, includeReReleaseYear: false, includeTracks: false, includeMetadata: false);
        context.Albums.Add(album);
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<AlbumLiteRow>> result = await sut.GetAlbumsLiteByArtistIdAsync(artist.Id, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        AlbumLiteRow row = Assert.Single(result.Value.Data);
        Assert.Equal(1975, row.ReleaseYear);
    }

    [Fact]
    public async Task GetAlbumsLiteByArtistIdAsync_WhenTheReleaseHasTypes_ShouldProjectThem()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-lite-releasetypes-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = _albumEntityFixture.Create(
            artistId: artist.Id, libraryId: libraryId, releaseType: MusicReleaseType.Single, includeTracks: false, includeMetadata: false);
        context.Albums.Add(album);
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<AlbumLiteRow>> result = await sut.GetAlbumsLiteByArtistIdAsync(artist.Id, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        AlbumLiteRow row = Assert.Single(result.Value.Data);
        Assert.NotNull(row.ReleaseTypes);
        Assert.Equal(MusicReleaseType.Single, Assert.Single(row.ReleaseTypes));
    }

    [Fact]
    public async Task GetAlbumsLiteByArtistIdAsync_WhenCoverArtworkExists_ShouldProjectTheCoverPath()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-lite-cover-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        context.Albums.Add(album);
        context.MusicArtwork.Add(
            _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: album.Id, artworkType: ArtworkType.Cover, fileName: "covers/opera.jpg"));
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<AlbumLiteRow>> result = await sut.GetAlbumsLiteByArtistIdAsync(artist.Id, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        AlbumLiteRow row = Assert.Single(result.Value.Data);
        Assert.Equal("covers/opera.jpg", row.CoverPath);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTheAlbumIsSoftDeleted_ShouldReturnNull()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-getbyid-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = await SeedAlbumAsync(context, libraryId, artist.Id, "Deleted Album");
        album.IsDeleted = true;
        album.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        await context.SaveChangesAsync();

        // Act
        Result<AlbumEntity?> result = await sut.GetByIdAsync(album.Id, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenItsArtistIsSoftDeleted_ShouldReturnNull()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-getbyid-deleted-artist-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = await SeedAlbumAsync(context, libraryId, artist.Id, "Album Of Deleted Artist");
        artist.IsDeleted = true;
        artist.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        await context.SaveChangesAsync();

        // Act
        Result<AlbumEntity?> result = await sut.GetByIdAsync(album.Id, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenATrackIsSoftDeleted_ShouldExcludeItFromTheGraph()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-getbyid-deleted-track-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = BuildFullAlbum(artist.Id, libraryId, "Album");
        TrackEntity activeTrack = album.Tracks[0];
        TrackEntity deletedTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        deletedTrack.IsDeleted = true;
        deletedTrack.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        album.Tracks = [activeTrack, deletedTrack];
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<AlbumEntity?> result = await sut.GetByIdAsync(album.Id, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(activeTrack.Id, Assert.Single(result.Value!.Tracks).Id);
    }

    [Fact]
    public async Task GetByArtistIdAsync_WhenAnAlbumIsSoftDeleted_ShouldExcludeIt()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-getbyartist-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity activeAlbum = await SeedAlbumAsync(context, libraryId, artist.Id, "Active Album");
        AlbumEntity deletedAlbum = await SeedAlbumAsync(context, libraryId, artist.Id, "Deleted Album");
        deletedAlbum.IsDeleted = true;
        deletedAlbum.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<AlbumEntity>> result = await sut.GetByArtistIdAsync(artist.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(activeAlbum.Id, Assert.Single(result.Value).Id);
    }

    [Fact]
    public async Task GetByArtistIdIncludingDeletedAsync_WhenCalled_ShouldReturnEveryAlbumOfTheArtistOrdered()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-getbyartist-including-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity betaAlbum = await SeedAlbumAsync(context, libraryId, artist.Id, "Beta");
        AlbumEntity deletedAlbum = await SeedAlbumAsync(context, libraryId, artist.Id, "Alpha");
        deletedAlbum.IsDeleted = true;
        deletedAlbum.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        ArtistEntity otherArtist = await SeedArtistAsync(context, libraryId);
        AlbumEntity albumOfAnotherArtist = await SeedAlbumAsync(context, libraryId, otherArtist.Id, "Gamma");
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<AlbumEntity>> result = await sut.GetByArtistIdIncludingDeletedAsync(artist.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal([deletedAlbum.Id, betaAlbum.Id], result.Value.Select(album => album.Id));
        Assert.DoesNotContain(result.Value, album => album.Id == albumOfAnotherArtist.Id);
    }

    [Fact]
    public async Task GetIncludingDeletedByIdAsync_WhenTheAlbumIsSoftDeleted_ShouldReturnIt()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-get-including-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = await SeedAlbumAsync(context, libraryId, artist.Id, "Deleted Album");
        album.IsDeleted = true;
        album.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        await context.SaveChangesAsync();

        // Act
        Result<AlbumEntity?> result = await sut.GetIncludingDeletedByIdAsync(album.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(album.Id, result.Value!.Id);
        Assert.True(result.Value.IsDeleted);
    }

    [Fact]
    public async Task GetSoftDeletedByLibraryIdAsync_WhenCalled_ShouldReturnOnlyDeletedAlbumsOfVisibleArtistsOrdered()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-getdeleted-library-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        DateTime deletedOnUtc = _mockDateTimeProvider.UtcNow;
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity olderDeletion = await SeedAlbumAsync(context, libraryId, artist.Id, "Older");
        olderDeletion.IsDeleted = true;
        olderDeletion.DeletedOnUtc = deletedOnUtc.AddHours(-2);
        AlbumEntity newerDeletion = await SeedAlbumAsync(context, libraryId, artist.Id, "Newer");
        newerDeletion.IsDeleted = true;
        newerDeletion.DeletedOnUtc = deletedOnUtc.AddHours(-1);
        AlbumEntity activeAlbum = await SeedAlbumAsync(context, libraryId, artist.Id, "Active");
        ArtistEntity deletedArtist = await SeedArtistAsync(context, libraryId);
        AlbumEntity deletedAlbumOfDeletedArtist = await SeedAlbumAsync(context, libraryId, deletedArtist.Id, "Hidden");
        deletedAlbumOfDeletedArtist.IsDeleted = true;
        deletedAlbumOfDeletedArtist.DeletedOnUtc = deletedOnUtc;
        deletedArtist.IsDeleted = true;
        deletedArtist.DeletedOnUtc = deletedOnUtc;
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<AlbumEntity>> result = await sut.GetSoftDeletedByLibraryIdAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal([newerDeletion.Id, olderDeletion.Id], result.Value.Select(album => album.Id));
        Assert.DoesNotContain(result.Value, album => album.Id == deletedAlbumOfDeletedArtist.Id);
    }

    [Fact]
    public async Task SoftDeleteByIdAsync_WhenAnotherVisibleAlbumOfTheArtistRemains_ShouldPersistTheDeletionStateAndProviderTime()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-softdelete-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = await SeedAlbumAsync(context, libraryId, artist.Id, "Album");
        await SeedAlbumAsync(context, libraryId, artist.Id, "Kept Album");

        // Act
        Result<Updated> result = await sut.SoftDeleteByIdAsync(album.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        AlbumEntity storedAlbum = await context.Albums.AsNoTracking().SingleAsync(candidate => candidate.Id == album.Id);
        Assert.True(storedAlbum.IsDeleted);
        Assert.Equal(_mockDateTimeProvider.UtcNow, storedAlbum.DeletedOnUtc);
    }

    [Fact]
    public async Task SoftDeleteByIdAsync_WhenAlbumIsTheOnlyVisibleAlbumOfTheArtist_ShouldReturnArtistMustHaveAtLeastOneAlbumAndKeepItVisible()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-softdelete-last-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = await SeedAlbumAsync(context, libraryId, artist.Id, "Album");

        // Act
        Result<Updated> result = await sut.SoftDeleteByIdAsync(album.Id, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistMustHaveAtLeastOneAlbum, result.FirstError);
        AlbumEntity storedAlbum = await context.Albums.AsNoTracking().SingleAsync(candidate => candidate.Id == album.Id);
        Assert.False(storedAlbum.IsDeleted);
        Assert.Null(storedAlbum.DeletedOnUtc);
    }

    [Fact]
    public async Task SoftDeleteByIdAsync_WhenTheOtherAlbumsOfTheArtistAreDeleted_ShouldReturnArtistMustHaveAtLeastOneAlbum()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-softdelete-deleted-siblings-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = await SeedAlbumAsync(context, libraryId, artist.Id, "Album");
        AlbumEntity deletedAlbum = await SeedAlbumAsync(context, libraryId, artist.Id, "Deleted Album");
        deletedAlbum.IsDeleted = true;
        deletedAlbum.DeletedOnUtc = _mockDateTimeProvider.UtcNow.AddHours(-1);
        await context.SaveChangesAsync();

        // Act
        Result<Updated> result = await sut.SoftDeleteByIdAsync(album.Id, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistMustHaveAtLeastOneAlbum, result.FirstError);
        AlbumEntity storedAlbum = await context.Albums.AsNoTracking().SingleAsync(candidate => candidate.Id == album.Id);
        Assert.False(storedAlbum.IsDeleted);
    }

    [Fact]
    public async Task SoftDeleteByIdAsync_WhenAlbumIsAlreadyDeleted_ShouldReturnAlbumNotFound()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-softdelete-already-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = await SeedAlbumAsync(context, libraryId, artist.Id, "Album");
        await SeedAlbumAsync(context, libraryId, artist.Id, "Visible Album");
        DateTime storedDeletedOnUtc = _mockDateTimeProvider.UtcNow.AddHours(-1);
        album.IsDeleted = true;
        album.DeletedOnUtc = storedDeletedOnUtc;
        await context.SaveChangesAsync();

        // Act
        Result<Updated> result = await sut.SoftDeleteByIdAsync(album.Id, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        AlbumEntity storedAlbum = await context.Albums.AsNoTracking().SingleAsync(candidate => candidate.Id == album.Id);
        Assert.Equal(storedDeletedOnUtc, storedAlbum.DeletedOnUtc);
    }

    [Fact]
    public async Task SoftDeleteByIdAsync_WhenAlbumDoesNotExist_ShouldReturnAlbumNotFound()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-softdelete-missing-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        // Act
        Result<Updated> result = await sut.SoftDeleteByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
    }

    [Fact]
    public async Task SoftDeleteByIdAsync_WhenAnotherWriterAlreadyDeletedTheOtherLastAlbum_ShouldReturnArtistMustHaveAtLeastOneAlbum()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-softdelete-race-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext firstContext = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        firstContext.Database.EnsureCreated();
        using NemeraDbContext secondContext = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        AlbumRepository firstRepository = CreateAlbumRepository(firstContext);
        AlbumRepository secondRepository = CreateAlbumRepository(secondContext);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(firstContext, libraryId);
        AlbumEntity firstAlbum = await SeedAlbumAsync(firstContext, libraryId, artist.Id, "First Album");
        AlbumEntity secondAlbum = await SeedAlbumAsync(firstContext, libraryId, artist.Id, "Second Album");

        // The second writer reads the state a preceding check would have seen: both albums visible. The write must not trust that read.
        int visibleAlbumsSeenBySecondWriter = await secondContext.Albums.CountAsync(album => album.ArtistId == artist.Id && !album.IsDeleted);
        Assert.Equal(2, visibleAlbumsSeenBySecondWriter);

        // Act
        Result<Updated> firstResult = await firstRepository.SoftDeleteByIdAsync(firstAlbum.Id, CancellationToken.None);
        Result<Updated> secondResult = await secondRepository.SoftDeleteByIdAsync(secondAlbum.Id, CancellationToken.None);

        // Assert
        Assert.False(firstResult.IsFailure);
        Assert.True(secondResult.IsFailure);
        Assert.Equal(Errors.Music.ArtistMustHaveAtLeastOneAlbum, secondResult.FirstError);
        int remainingVisibleAlbums = await secondContext.Albums.CountAsync(album => album.ArtistId == artist.Id && !album.IsDeleted);
        Assert.Equal(1, remainingVisibleAlbums);
    }

    [Fact]
    public async Task RestoreSoftDeletedByIdAsync_WhenCalled_ShouldPersistTheClearedDeletionState()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-restore-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = await SeedAlbumAsync(context, libraryId, artist.Id, "Album");
        album.IsDeleted = true;
        album.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        await context.SaveChangesAsync();

        // Act
        Result<Updated> result = await sut.RestoreSoftDeletedByIdAsync(album.Id, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        AlbumEntity storedAlbum = await context.Albums.AsNoTracking().SingleAsync(candidate => candidate.Id == album.Id);
        Assert.False(storedAlbum.IsDeleted);
        Assert.Null(storedAlbum.DeletedOnUtc);
    }

    [Fact]
    public async Task UpdateAsync_WhenTheAlbumIsSoftDeleted_ShouldReturnAlbumNotFound()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-update-deleted-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = await SeedAlbumAsync(context, libraryId, artist.Id, "Album");
        album.IsDeleted = true;
        album.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        await context.SaveChangesAsync();

        AlbumEntity data = _albumEntityFixture.Create(id: album.Id, artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);

        // Act
        Result<Updated> result = await sut.UpdateAsync(data, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
    }

    [Fact]
    public async Task UpdateAsync_WhenItsArtistIsSoftDeleted_ShouldReturnAlbumNotFound()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-update-deleted-artist-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = await SeedAlbumAsync(context, libraryId, artist.Id, "Album");
        artist.IsDeleted = true;
        artist.DeletedOnUtc = _mockDateTimeProvider.UtcNow;
        await context.SaveChangesAsync();

        AlbumEntity data = _albumEntityFixture.Create(id: album.Id, artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);

        // Act
        Result<Updated> result = await sut.UpdateAsync(data, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
    }

    [Fact]
    public async Task UpdateAsync_WhenIncomingDataIsMarkedDeleted_ShouldPreserveTheActiveRowsDeletionState()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-albumrepo-update-preserve-deletion-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using NemeraDbContext context = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        AlbumRepository sut = CreateAlbumRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId);
        AlbumEntity album = await SeedAlbumAsync(context, libraryId, artist.Id, "Album");
        AlbumEntity data = await LoadDetachedAlbumAsync(context, album.Id);
        data.Title = "A Night at the Opera";
        data.IsDeleted = true;
        data.DeletedOnUtc = _mockDateTimeProvider.UtcNow;

        // Act
        Result<Updated> result = await sut.UpdateAsync(data, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        AlbumEntity storedAlbum = await context.Albums.AsNoTracking().SingleAsync(candidate => candidate.Id == album.Id);
        Assert.Equal("A Night at the Opera", storedAlbum.Title);
        Assert.False(storedAlbum.IsDeleted);
        Assert.Null(storedAlbum.DeletedOnUtc);
    }

    /// <summary>
    /// Creates an album repository backed by the provided context, wiring the real track repository.
    /// </summary>
    /// <param name="context">The context the repository operates on.</param>
    /// <returns>The created repository.</returns>
    private AlbumRepository CreateAlbumRepository(NemeraDbContext context)
    {
        return new AlbumRepository(context, new TrackRepository(context, _mockDateTimeProvider), _mockDateTimeProvider);
    }

    /// <summary>
    /// Seeds an artist with no albums or contributors, so that the required foreign key of an album is satisfied.
    /// </summary>
    /// <param name="context">The context the artist is persisted through.</param>
    /// <param name="libraryId">The Id of the library that owns the artist.</param>
    /// <returns>The persisted artist.</returns>
    private async Task<ArtistEntity> SeedArtistAsync(NemeraDbContext context, Guid libraryId)
    {
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        return artist;
    }

    /// <summary>
    /// Seeds an album that belongs to the artist identified by <paramref name="artistId"/>.
    /// </summary>
    /// <param name="context">The context the album is persisted through.</param>
    /// <param name="libraryId">The Id of the library that owns the album.</param>
    /// <param name="artistId">The Id of the artist that owns the album.</param>
    /// <param name="title">The title of the album.</param>
    /// <returns>The persisted album.</returns>
    private async Task<AlbumEntity> SeedAlbumAsync(NemeraDbContext context, Guid libraryId, Guid artistId, string title)
    {
        AlbumEntity album = _albumEntityFixture.Create(artistId: artistId, libraryId: libraryId, title: title, includeTracks: false, includeMetadata: false);
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        return album;
    }

    /// <summary>
    /// Seeds an album with one track, including every related collection deterministically.
    /// </summary>
    /// <param name="context">The context the album is persisted through.</param>
    /// <param name="libraryId">The Id of the library that owns the album.</param>
    /// <param name="artistId">The Id of the artist that owns the album.</param>
    /// <param name="title">The title of the album.</param>
    /// <returns>The persisted album.</returns>
    private async Task<AlbumEntity> SeedFullAlbumAsync(NemeraDbContext context, Guid libraryId, Guid artistId, string title)
    {
        AlbumEntity album = BuildFullAlbum(artistId, libraryId, title);
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        return album;
    }

    /// <summary>
    /// Builds an album with one track, including every related collection deterministically, without persisting it.
    /// </summary>
    /// <param name="artistId">The Id of the artist that owns the album.</param>
    /// <param name="libraryId">The Id of the library that owns the album.</param>
    /// <param name="title">The title of the album.</param>
    /// <returns>The created album.</returns>
    private AlbumEntity BuildFullAlbum(Guid artistId, Guid libraryId, string title)
    {
        AlbumEntity album = _albumEntityFixture.Create(artistId: artistId, libraryId: libraryId, title: title, includeTracks: false, includeMetadata: false);
        album.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100)];
        album.Tags = [_tagEntityFixture.Create(name: "album-tag-1"), _tagEntityFixture.Create(name: "album-tag-2")];
        album.Genres = [_genreEntityFixture.Create(name: "album-genre-1"), _genreEntityFixture.Create(name: "album-genre-2")];
        album.Contributors = [_albumContributorEntityFixture.Create(albumId: album.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Producer)];
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        track.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.User, voteCount: 10)];
        track.Tags = [_tagEntityFixture.Create(name: "track-tag-1"), _tagEntityFixture.Create(name: "track-tag-2")];
        track.Genres = [_genreEntityFixture.Create(name: "track-genre-1"), _genreEntityFixture.Create(name: "track-genre-2")];
        track.Moods = [_trackMoodEntityFixture.Create(name: "Calm"), _trackMoodEntityFixture.Create(name: "Upbeat")];
        track.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678"), _trackIsrcEntityFixture.Create(value: "GBAYE0000001")];
        track.Contributors = [_trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)];
        album.Tracks = [track];
        return album;
    }

    /// <summary>
    /// Loads a detached copy of a stored album with the whole aggregate, so that it can be handed to the repository as the desired state of an edit.
    /// </summary>
    /// <param name="context">The context that tracks the stored album.</param>
    /// <param name="albumId">The Id of the album to load.</param>
    /// <returns>The detached copy of the album.</returns>
    private static async Task<AlbumEntity> LoadDetachedAlbumAsync(NemeraDbContext context, Guid albumId)
    {
        return await context.Albums
            .AsNoTracking()
            .Include(album => album.Ratings)
            .Include(album => album.Tags)
            .Include(album => album.Genres)
            .Include(album => album.Contributors)
            .Include(album => album.Tracks)
                .ThenInclude(track => track.Ratings)
            .Include(album => album.Tracks)
                .ThenInclude(track => track.Genres)
            .Include(album => album.Tracks)
                .ThenInclude(track => track.Tags)
            .Include(album => album.Tracks)
                .ThenInclude(track => track.Moods)
            .Include(album => album.Tracks)
                .ThenInclude(track => track.Isrcs)
            .Include(album => album.Tracks)
                .ThenInclude(track => track.Contributors)
            .AsSplitQuery()
            .FirstAsync(album => album.Id == albumId);
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
}
