#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DTO.Filtering;
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
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.MusicLibrary;

/// <summary>
/// Contains integration tests for the <see cref="ArtistRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistRepositoryTests
{
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly ArtistContributorEntityFixture _artistContributorEntityFixture = new();
    private readonly AlbumContributorEntityFixture _albumContributorEntityFixture = new();
    private readonly TrackContributorEntityFixture _trackContributorEntityFixture = new();
    private readonly AudioRatingEntityFixture _audioRatingEntityFixture = new();
    private readonly TrackMoodEntityFixture _trackMoodEntityFixture = new();
    private readonly TrackIsrcEntityFixture _trackIsrcEntityFixture = new();
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly GenreEntityFixture _genreEntityFixture = new();
    private readonly PaginationDataDtoFixture _paginationDataDtoFixture = new();
    private readonly LibraryFilterDtoFixture _libraryFilterDtoFixture = new();

    [Fact]
    public async Task InsertAsync_WhenCalledWithAValidArtist_ShouldPersistTheArtistAndItsAggregate()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-insert-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        artist.Contributors =
            [_artistContributorEntityFixture.Create(artistId: artist.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)];
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Tags = [_tagEntityFixture.Create(name: "InsertAlbumTag")];
        album.Genres = [_genreEntityFixture.Create(name: "InsertAlbumGenre")];
        album.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100)];
        album.Contributors = [_albumContributorEntityFixture.Create(albumId: album.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Producer)];
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "InsertTrackTag")];
        track.Genres = [_genreEntityFixture.Create(name: "InsertTrackGenre")];
        track.Moods = [_trackMoodEntityFixture.Create(name: "Calm")];
        track.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678")];
        track.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.User, voteCount: 10)];
        track.Contributors = [_trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)];
        album.Tracks = [track];
        artist.Albums = [album];

        // Act
        Result<Created> result = await sut.InsertAsync(artist, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        ArtistEntity? storedArtist = await context.Artists
            .AsNoTracking()
            .Include(candidate => candidate.Contributors)
            .Include(candidate => candidate.Albums)
                .ThenInclude(candidate => candidate.Ratings)
            .Include(candidate => candidate.Albums)
                .ThenInclude(candidate => candidate.Tags)
            .Include(candidate => candidate.Albums)
                .ThenInclude(candidate => candidate.Genres)
            .Include(candidate => candidate.Albums)
                .ThenInclude(candidate => candidate.Contributors)
            .Include(candidate => candidate.Albums)
                .ThenInclude(candidate => candidate.Tracks)
                    .ThenInclude(candidate => candidate.Tags)
            .Include(candidate => candidate.Albums)
                .ThenInclude(candidate => candidate.Tracks)
                    .ThenInclude(candidate => candidate.Genres)
            .Include(candidate => candidate.Albums)
                .ThenInclude(candidate => candidate.Tracks)
                    .ThenInclude(candidate => candidate.Moods)
            .Include(candidate => candidate.Albums)
                .ThenInclude(candidate => candidate.Tracks)
                    .ThenInclude(candidate => candidate.Isrcs)
            .AsSplitQuery()
            .FirstOrDefaultAsync(candidate => candidate.Id == artist.Id);
        Assert.NotNull(storedArtist);
        Assert.Single(storedArtist!.Contributors);
        AlbumEntity storedAlbum = Assert.Single(storedArtist.Albums);
        Assert.Single(storedAlbum.Ratings);
        Assert.Single(storedAlbum.Tags);
        Assert.Single(storedAlbum.Genres);
        Assert.Single(storedAlbum.Contributors);
        TrackEntity storedTrack = Assert.Single(storedAlbum.Tracks);
        Assert.Equal("/music/queen/bohemian-rhapsody.flac", storedTrack.Path);
        Assert.Single(storedTrack.Tags);
        Assert.Single(storedTrack.Genres);
        Assert.Single(storedTrack.Moods);
        Assert.Single(storedTrack.Isrcs);
        // Each tag and genre referenced by the aggregate is stored exactly once.
        Assert.Single(await context.Set<TagEntity>().Where(tag => tag.Name == "InsertAlbumTag").ToListAsync());
        Assert.Single(await context.Set<TagEntity>().Where(tag => tag.Name == "InsertTrackTag").ToListAsync());
        Assert.Single(await context.Set<GenreEntity>().Where(genre => genre.Name == "InsertAlbumGenre").ToListAsync());
        Assert.Single(await context.Set<GenreEntity>().Where(genre => genre.Name == "InsertTrackGenre").ToListAsync());
    }

    [Fact]
    public async Task InsertAsync_WhenArtistWithTheSameIdAlreadyExists_ShouldReturnArtistAlreadyExists()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-insert-id-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        ArtistEntity existingArtist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        context.Artists.Add(existingArtist);
        await context.SaveChangesAsync();

        ArtistEntity artist = _artistEntityFixture.Create(id: existingArtist.Id, includeAlbums: false, includeContributors: false);

        // Act
        Result<Created> result = await sut.InsertAsync(artist, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task InsertAsync_WhenAnotherArtistOfTheSameLibraryHasTheSameName_ShouldReturnArtistAlreadyExists()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-insert-name-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity existingArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Queen", includeAlbums: false, includeContributors: false);
        context.Artists.Add(existingArtist);
        await context.SaveChangesAsync();

        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, name: "Queen", includeAlbums: false, includeContributors: false);

        // Act
        Result<Created> result = await sut.InsertAsync(artist, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task InsertAsync_WhenAnotherLibraryHasAnArtistWithTheSameName_ShouldPersistTheArtist()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-insert-otherlibrary-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        ArtistEntity existingArtist = _artistEntityFixture.Create(libraryId: Guid.NewGuid(), name: "Queen", includeAlbums: false, includeContributors: false);
        context.Artists.Add(existingArtist);
        await context.SaveChangesAsync();

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, name: "Queen", includeAlbums: false, includeContributors: false);

        // Act
        Result<Created> result = await sut.InsertAsync(artist, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        ArtistEntity? storedArtist = await context.Artists.AsNoTracking().FirstOrDefaultAsync(candidate => candidate.Id == artist.Id);
        Assert.NotNull(storedArtist);
        Assert.Equal(libraryId, storedArtist!.LibraryId);
    }

    [Fact]
    public async Task InsertAsync_WhenTwoTracksOfTheSameRequestShareAPath_ShouldReturnTrackAlreadyExists()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-insert-duprequest-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        ArtistEntity artist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: artist.LibraryId, includeTracks: false, includeMetadata: false);
        album.Tracks =
        [
            _trackEntityFixture.Create(albumId: album.Id, libraryId: artist.LibraryId, path: "/music/queen/duplicate.flac", includeMetadata: false),
            _trackEntityFixture.Create(albumId: album.Id, libraryId: artist.LibraryId, path: "/music/queen/duplicate.flac", includeMetadata: false)
        ];
        artist.Albums = [album];

        // Act
        Result<Created> result = await sut.InsertAsync(artist, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task InsertAsync_WhenATrackPathIsAlreadyStoredInTheLibrary_ShouldReturnTrackAlreadyExists()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-insert-storedpath-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity existingArtist = await SeedArtistAsync(context, libraryId, "Existing");
        AlbumEntity existingAlbum = await SeedAlbumAsync(context, libraryId, existingArtist.Id);
        context.Tracks.Add(_trackEntityFixture.Create(
            albumId: existingAlbum.Id, libraryId: libraryId, path: "/music/queen/we-will-rock-you.flac", includeMetadata: false));
        await context.SaveChangesAsync();

        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Tracks = [_trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/we-will-rock-you.flac", includeMetadata: false)];
        artist.Albums = [album];

        // Act
        Result<Created> result = await sut.InsertAsync(artist, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task InsertAsync_WhenAnotherLibraryHasATrackWithTheSamePath_ShouldPersistTheArtist()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-insert-otherlibrarypath-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid firstLibraryId = Guid.NewGuid();
        ArtistEntity firstArtist = await SeedArtistAsync(context, firstLibraryId, "First");
        AlbumEntity firstAlbum = await SeedAlbumAsync(context, firstLibraryId, firstArtist.Id);
        context.Tracks.Add(_trackEntityFixture.Create(
            albumId: firstAlbum.Id, libraryId: firstLibraryId, path: "/music/queen/we-are-the-champions.flac", includeMetadata: false));
        await context.SaveChangesAsync();

        Guid secondLibraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: secondLibraryId, includeAlbums: false, includeContributors: false);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: secondLibraryId, includeTracks: false, includeMetadata: false);
        album.Tracks = [_trackEntityFixture.Create(albumId: album.Id, libraryId: secondLibraryId, path: "/music/queen/we-are-the-champions.flac", includeMetadata: false)];
        artist.Albums = [album];

        // Act
        Result<Created> result = await sut.InsertAsync(artist, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        ArtistEntity? storedArtist = await context.Artists.AsNoTracking().FirstOrDefaultAsync(candidate => candidate.Id == artist.Id);
        Assert.NotNull(storedArtist);
        Assert.Equal(secondLibraryId, storedArtist!.LibraryId);
    }

    [Fact]
    public async Task InsertAsync_WhenTagsAndGenresAlreadyExistByName_ShouldReuseTheStoredRows()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-insert-reuse-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        context.Set<TagEntity>().Add(_tagEntityFixture.Create(name: "ExistingTag"));
        context.Set<GenreEntity>().Add(_genreEntityFixture.Create(name: "ExistingGenre"));
        await context.SaveChangesAsync();

        ArtistEntity artist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: artist.LibraryId, includeTracks: false, includeMetadata: false);
        album.Tags = [_tagEntityFixture.Create(name: "ExistingTag")];
        album.Genres = [_genreEntityFixture.Create(name: "ExistingGenre")];
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: artist.LibraryId, includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "ExistingTag")];
        track.Genres = [_genreEntityFixture.Create(name: "ExistingGenre")];
        album.Tracks = [track];
        artist.Albums = [album];

        // Act
        Result<Created> result = await sut.InsertAsync(artist, CancellationToken.None);
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
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-insert-shared-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        ArtistEntity artist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: artist.LibraryId, includeTracks: false, includeMetadata: false);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: artist.LibraryId, includeMetadata: false);
        album.Tags = [_tagEntityFixture.Create(name: "SharedNewTag")];
        album.Genres = [_genreEntityFixture.Create(name: "SharedNewGenre")];
        track.Tags = [_tagEntityFixture.Create(name: "SharedNewTag")];
        track.Genres = [_genreEntityFixture.Create(name: "SharedNewGenre")];
        album.Tracks = [track];
        artist.Albums = [album];

        // Act
        Result<Created> result = await sut.InsertAsync(artist, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        // The name shared by the album and its tracks is persisted exactly once.
        Assert.Single(await context.Set<TagEntity>().Where(tag => tag.Name == "SharedNewTag").ToListAsync());
        Assert.Single(await context.Set<GenreEntity>().Where(genre => genre.Name == "SharedNewGenre").ToListAsync());
    }

    [Fact]
    public async Task UpdateAsync_WhenANewTagAndGenreNameIsSharedBetweenTheAlbumAndItsTrack_ShouldPersistItOnce()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-update-shared-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        album.Tags = [];
        album.Genres = [];
        track.Tags = [];
        track.Genres = [];
        album.Tracks = [track];
        artist.Albums = [album];
        context.Artists.Add(artist);
        await context.SaveChangesAsync();

        ArtistEntity incoming = await LoadDetachedArtistAsync(context, artist.Id);
        AlbumEntity incomingAlbum = Assert.Single(incoming.Albums);
        TrackEntity incomingTrack = Assert.Single(incomingAlbum.Tracks);
        incomingAlbum.Tags = [_tagEntityFixture.Create(name: "SharedNewTag")];
        incomingAlbum.Genres = [_genreEntityFixture.Create(name: "SharedNewGenre")];
        incomingTrack.Tags = [_tagEntityFixture.Create(name: "SharedNewTag")];
        incomingTrack.Genres = [_genreEntityFixture.Create(name: "SharedNewGenre")];

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        // The name shared by the album and its track is persisted exactly once.
        Assert.Single(await context.Set<TagEntity>().Where(tag => tag.Name == "SharedNewTag").ToListAsync());
        Assert.Single(await context.Set<GenreEntity>().Where(genre => genre.Name == "SharedNewGenre").ToListAsync());
    }

    [Fact]
    public async Task UpdateAsync_WhenNothingChanged_ShouldNotChangeAnyAuditColumn()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-update-noop-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        artist.Contributors =
            [_artistContributorEntityFixture.Create(artistId: artist.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)];
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Tags = [_tagEntityFixture.Create(name: "NoopAlbumTag")];
        album.Genres = [_genreEntityFixture.Create(name: "NoopAlbumGenre")];
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "NoopTrackTag")];
        track.Genres = [_genreEntityFixture.Create(name: "NoopTrackGenre")];
        album.Tracks = [track];
        artist.Albums = [album];
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        Guid contributorId = artist.Contributors[0].Id;
        DateTime contributorCreatedOnUtc = artist.Contributors[0].CreatedOnUtc;
        ArtistEntity incoming = await LoadDetachedArtistAsync(context, artist.Id);

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        ArtistEntity? storedArtist = await context.Artists
            .AsNoTracking()
            .Include(candidate => candidate.Contributors)
            .Include(candidate => candidate.Albums)
                .ThenInclude(candidate => candidate.Tracks)
            .AsSplitQuery()
            .FirstOrDefaultAsync(candidate => candidate.Id == artist.Id);
        Assert.NotNull(storedArtist);
        Assert.Null(storedArtist!.UpdatedOnUtc);
        ArtistContributorEntity storedContributor = Assert.Single(storedArtist.Contributors);
        Assert.Equal(contributorId, storedContributor.Id);
        Assert.Equal(contributorCreatedOnUtc, storedContributor.CreatedOnUtc);
        // The tag and genre rows of the album and its track are left untouched.
        Assert.Single(await context.Set<TagEntity>().Where(tag => tag.Name == "NoopAlbumTag").ToListAsync());
        Assert.Single(await context.Set<TagEntity>().Where(tag => tag.Name == "NoopTrackTag").ToListAsync());
        Assert.Single(await context.Set<GenreEntity>().Where(genre => genre.Name == "NoopAlbumGenre").ToListAsync());
        Assert.Single(await context.Set<GenreEntity>().Where(genre => genre.Name == "NoopTrackGenre").ToListAsync());
    }

    [Fact]
    public async Task UpdateAsync_WhenOnlyTheScalarValuesChanged_ShouldStampTheArtistAndKeepChildIdentity()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-update-scalars-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        artist.Contributors =
            [_artistContributorEntityFixture.Create(artistId: artist.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)];
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        Guid contributorId = artist.Contributors[0].Id;
        DateTime contributorCreatedOnUtc = artist.Contributors[0].CreatedOnUtc;
        ArtistEntity incoming = await LoadDetachedArtistAsync(context, artist.Id);
        incoming.Name = "Freddie Mercury";

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        ArtistEntity? storedArtist = await context.Artists.AsNoTracking().Include(candidate => candidate.Contributors).FirstOrDefaultAsync(candidate => candidate.Id == artist.Id);
        Assert.NotNull(storedArtist);
        Assert.Equal("Freddie Mercury", storedArtist!.Name);
        // Only the edited artist is stamped, the child rows keep their identity and their audit columns.
        Assert.Equal(utcNow, storedArtist.UpdatedOnUtc);
        Assert.Equal(userId, storedArtist.UpdatedBy);
        ArtistContributorEntity storedContributor = Assert.Single(storedArtist.Contributors);
        Assert.Equal(contributorId, storedContributor.Id);
        Assert.Equal(contributorCreatedOnUtc, storedContributor.CreatedOnUtc);
    }

    [Fact]
    public async Task UpdateAsync_WhenAContributorIsRemoved_ShouldDeleteOnlyThatParticipationAndKeepTheOtherIdentity()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-update-removecontributor-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid keptMediaContributorId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        ArtistContributorEntity keptContributor = _artistContributorEntityFixture.Create(
            artistId: artist.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Producer);
        ArtistContributorEntity removedContributor = _artistContributorEntityFixture.Create(
            artistId: artist.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Composer);
        artist.Contributors = [keptContributor, removedContributor];
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        Guid keptContributorId = keptContributor.Id;
        ArtistEntity incoming = await LoadDetachedArtistAsync(context, artist.Id);
        incoming.Contributors = [incoming.Contributors.Single(contributor => contributor.MediaContributorId == keptMediaContributorId)];

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        List<ArtistContributorEntity> storedContributors = await context.Set<ArtistContributorEntity>()
            .AsNoTracking()
            .Where(contributor => contributor.ArtistId == artist.Id)
            .ToListAsync();
        ArtistContributorEntity storedContributor = Assert.Single(storedContributors);
        Assert.Equal(keptContributorId, storedContributor.Id);
        Assert.Equal(keptMediaContributorId, storedContributor.MediaContributorId);
    }

    [Fact]
    public async Task UpdateAsync_WhenAnAlbumAndItsTrackAreAdded_ShouldPersistThem()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-update-addalbum-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        ArtistEntity incoming = await LoadDetachedArtistAsync(context, artist.Id);
        AlbumEntity addedAlbum = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        addedAlbum.Tracks =
            [_trackEntityFixture.Create(albumId: addedAlbum.Id, libraryId: libraryId, path: "/music/queen/love-of-my-life.flac", includeMetadata: false)];
        incoming.Albums = [addedAlbum];

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        AlbumEntity? storedAlbum = await context.Albums.AsNoTracking().Include(album => album.Tracks).FirstOrDefaultAsync(album => album.Id == addedAlbum.Id);
        Assert.NotNull(storedAlbum);
        TrackEntity storedTrack = Assert.Single(storedAlbum!.Tracks);
        Assert.Equal("/music/queen/love-of-my-life.flac", storedTrack.Path);
    }

    [Fact]
    public async Task UpdateAsync_WhenANewTrackPathIsAlreadyStoredInTheLibrary_ShouldReturnTrackAlreadyExists()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-update-dup-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumAsync(context, libraryId, artist.Id);
        context.Tracks.Add(_trackEntityFixture.Create(
            albumId: album.Id, libraryId: libraryId, path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false));
        await context.SaveChangesAsync();

        ArtistEntity incoming = await LoadDetachedArtistAsync(context, artist.Id);
        AlbumEntity incomingAlbum = incoming.Albums.Single();
        TrackEntity addedTrack = _trackEntityFixture.Create(
            albumId: album.Id, libraryId: libraryId, path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false);
        incomingAlbum.Tracks.Add(addedTrack);

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackAlreadyExists, result.FirstError);
        Assert.Equal(1, await context.Tracks.CountAsync(candidate => candidate.LibraryId == libraryId));
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenArtistExists_ShouldDeleteTheArtistAndItsChildren()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-delete-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        AlbumEntity album = await SeedAlbumAsync(context, libraryId, artist.Id);
        context.Tracks.Add(_trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false));
        await context.SaveChangesAsync();

        // Act
        Result<Deleted> result = await sut.DeleteByIdAsync(artist.Id, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        Assert.Null(await context.Artists.AsNoTracking().FirstOrDefaultAsync(candidate => candidate.Id == artist.Id));
        Assert.Empty(await context.Albums.AsNoTracking().Where(candidate => candidate.ArtistId == artist.Id).ToListAsync());
        Assert.Empty(await context.Tracks.AsNoTracking().Where(candidate => candidate.LibraryId == libraryId).ToListAsync());
    }

    [Fact]
    public async Task GetByIdAsync_WhenArtistExists_ShouldIncludeTheNavigationProperties()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-getbyid-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedFullArtistAsync(context, libraryId);

        // Act
        Result<ArtistEntity?> result = await sut.GetByIdAsync(artist.Id, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ArtistEntity retrievedArtist = result.Value!;
        Assert.Single(retrievedArtist.Contributors);
        AlbumEntity retrievedAlbum = Assert.Single(retrievedArtist.Albums);
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
    public async Task GetByIdAsync_WhenEntitiesShouldNotBeTracked_ShouldReturnAnUntrackedArtist()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-getbyid-notracking-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = await SeedArtistAsync(context, libraryId, "Queen");
        context.ChangeTracker.Clear();

        // Act
        Result<ArtistEntity?> result = await sut.GetByIdAsync(
            artist.Id, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(context.ChangeTracker.Entries<ArtistEntity>());
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldReturnOnlyTheLibraryArtistsWithPagination()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-getall-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        List<ArtistEntity> artists = _artistEntityFixture.CreateMany(3);
        foreach (ArtistEntity artist in artists)
        {
            artist.LibraryId = libraryId;
            artist.Albums = [];
            artist.Contributors = [];
        }
        ArtistEntity artistOfAnotherLibrary = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        context.Artists.AddRange(artists);
        context.Artists.Add(artistOfAnotherLibrary);
        await context.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistEntity>> result = await sut.GetAllAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(3, result.Value.Data.Count);
        Assert.DoesNotContain(result.Value.Data, artist => artist.Id == artistOfAnotherLibrary.Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenTheSearchTermDiffersInCase_ShouldMatchCaseInsensitively()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-getall-search-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity matchingArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Queens of the Stone Age", includeAlbums: false, includeContributors: false);
        ArtistEntity otherArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Radiohead", includeAlbums: false, includeContributors: false);
        context.Artists.AddRange(matchingArtist, otherArtist);
        await context.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId, searchTerm: "QUEENS");

        // Act
        Result<PaginatedResultDto<ArtistEntity>> result = await sut.GetAllAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ArtistEntity retrievedArtist = Assert.Single(result.Value.Data);
        Assert.Equal(matchingArtist.Id, retrievedArtist.Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenEntitiesShouldNotBeTracked_ShouldReturnUntrackedArtists()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-getall-notracking-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistEntity>> result = await sut.GetAllAsync(
            null, null, null, filter, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value.Data);
        Assert.Empty(context.ChangeTracker.Entries<ArtistEntity>());
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenPaginationDataIsNull_ShouldReturnTheArtistLiteRowsOrdered()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-lite-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity betaArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Beta", includeAlbums: false, includeContributors: false);
        ArtistEntity alphaArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Alpha", includeAlbums: false, includeContributors: false);
        context.Artists.AddRange(betaArtist, alphaArtist);
        await context.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistLiteRow>> result = await sut.GetAllLiteAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal([alphaArtist.Id, betaArtist.Id], result.Value.Data.Select(row => row.Id));
        Assert.Equal([alphaArtist.Name, betaArtist.Name], result.Value.Data.Select(row => row.Name));
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(1, result.Value.CurrentPage);
        Assert.Equal(2, result.Value.PerPage);
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenEntitiesShouldNotBeTracked_ShouldReturnUntrackedRows()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-getalllite-notracking-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistLiteRow>> result = await sut.GetAllLiteAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value.Data);
        Assert.Empty(context.ChangeTracker.Entries<ArtistEntity>());
    }

    [Fact]
    public async Task UpdateAsync_WhenManyTracksAreUpdated_ShouldNotReloadEachTrackIndividually()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-update-queries-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        SelectCountingInterceptor interceptor = new();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>()
            .UseSqlite(anchorConnection.ConnectionString)
            .AddInterceptors(interceptor)
            .Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid smallLibraryId = Guid.NewGuid();
        ArtistEntity smallArtist = SeedArtistWithAlbums(context, smallLibraryId, albumCount: 1, tracksPerAlbum: 1);
        await context.SaveChangesAsync();

        Guid largeLibraryId = Guid.NewGuid();
        ArtistEntity largeArtist = SeedArtistWithAlbums(context, largeLibraryId, albumCount: 4, tracksPerAlbum: 10);
        await context.SaveChangesAsync();

        ArtistEntity smallIncoming = await LoadDetachedArtistAsync(context, smallArtist.Id);
        ArtistEntity largeIncoming = await LoadDetachedArtistAsync(context, largeArtist.Id);

        // Act
        interceptor.Reset();
        Result<Updated> smallResult = await sut.UpdateAsync(smallIncoming, CancellationToken.None);
        await context.SaveChangesAsync();
        int smallSelectCount = interceptor.SelectCount;

        interceptor.Reset();
        Result<Updated> largeResult = await sut.UpdateAsync(largeIncoming, CancellationToken.None);
        await context.SaveChangesAsync();
        int largeSelectCount = interceptor.SelectCount;

        // Assert
        Assert.False(smallResult.IsFailure);
        Assert.False(largeResult.IsFailure);
        // The artist aggregate is loaded once and the children are applied to the already tracked instances, so the number of read queries
        // does not grow with the number of albums or tracks of the aggregate.
        Assert.True(largeSelectCount <= smallSelectCount + 2, $"The large aggregate issued {largeSelectCount} read queries, while the small aggregate issued {smallSelectCount}.");
    }

    [Fact]
    public async Task UpdateAsync_WhenNothingChanged_ShouldNotDuplicateAnyReconciledChildCollection()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-update-noop-collections-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        artist.Contributors =
        [
            _artistContributorEntityFixture.Create(artistId: artist.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals),
            _artistContributorEntityFixture.Create(artistId: artist.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Producer)
        ];
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Tags = [_tagEntityFixture.Create(name: $"noop-album-tag-{Guid.NewGuid():N}"), _tagEntityFixture.Create(name: $"noop-album-tag-two-{Guid.NewGuid():N}")];
        album.Genres = [_genreEntityFixture.Create(name: $"noop-album-genre-{Guid.NewGuid():N}"), _genreEntityFixture.Create(name: $"noop-album-genre-two-{Guid.NewGuid():N}")];
        album.Ratings =
        [
            _audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100),
            _audioRatingEntityFixture.Create(value: 5, maxValue: 5, source: AudioRatingSource.User, voteCount: 10)
        ];
        album.Contributors =
        [
            _albumContributorEntityFixture.Create(albumId: album.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Producer),
            _albumContributorEntityFixture.Create(albumId: album.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Composer)
        ];
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: $"noop-track-tag-{Guid.NewGuid():N}"), _tagEntityFixture.Create(name: $"noop-track-tag-two-{Guid.NewGuid():N}")];
        track.Genres = [_genreEntityFixture.Create(name: $"noop-track-genre-{Guid.NewGuid():N}"), _genreEntityFixture.Create(name: $"noop-track-genre-two-{Guid.NewGuid():N}")];
        track.Ratings =
        [
            _audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100),
            _audioRatingEntityFixture.Create(value: 3, maxValue: 5, source: AudioRatingSource.User, voteCount: 20)
        ];
        track.Moods = [_trackMoodEntityFixture.Create(name: "Calm"), _trackMoodEntityFixture.Create(name: "Upbeat")];
        track.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678"), _trackIsrcEntityFixture.Create(value: "GBAYE0000001")];
        track.Contributors =
        [
            _trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals),
            _trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Producer)
        ];
        album.Tracks = [track];
        artist.Albums = [album];
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        // Detach the seeded graph, so that the update must load the tracked aggregate through its include chain instead of returning the already populated seeded instances.
        context.ChangeTracker.Clear();

        ArtistEntity incoming = await LoadDetachedArtistAsync(context, artist.Id);

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        ArtistEntity? storedArtist = await context.Artists.AsNoTracking().Include(candidate => candidate.Contributors).FirstOrDefaultAsync(candidate => candidate.Id == artist.Id);
        Assert.NotNull(storedArtist);
        Assert.Equal(2, storedArtist!.Contributors.Count);
        AlbumEntity? storedAlbum = await context.Albums
            .AsNoTracking()
            .Include(candidate => candidate.Tags)
            .Include(candidate => candidate.Genres)
            .Include(candidate => candidate.Ratings)
            .Include(candidate => candidate.Contributors)
            .FirstOrDefaultAsync(candidate => candidate.Id == album.Id);
        Assert.NotNull(storedAlbum);
        Assert.Equal(2, storedAlbum!.Tags.Count);
        Assert.Equal(2, storedAlbum.Genres.Count);
        Assert.Equal(2, storedAlbum.Ratings.Count);
        Assert.Equal(2, storedAlbum.Contributors.Count);
        TrackEntity? storedTrack = await context.Tracks
            .AsNoTracking()
            .Include(candidate => candidate.Tags)
            .Include(candidate => candidate.Genres)
            .Include(candidate => candidate.Ratings)
            .Include(candidate => candidate.Moods)
            .Include(candidate => candidate.Isrcs)
            .Include(candidate => candidate.Contributors)
            .FirstOrDefaultAsync(candidate => candidate.Id == track.Id);
        Assert.NotNull(storedTrack);
        Assert.Equal(2, storedTrack!.Tags.Count);
        Assert.Equal(2, storedTrack.Genres.Count);
        Assert.Equal(2, storedTrack.Ratings.Count);
        Assert.Equal(2, storedTrack.Moods.Count);
        Assert.Equal(2, storedTrack.Isrcs.Count);
        Assert.Equal(2, storedTrack.Contributors.Count);
    }

    [Fact]
    public async Task UpdateAsync_WhenOneItemIsRemovedFromEachReconciledChildCollection_ShouldRemoveOnlyThoseItems()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-artistrepo-update-remove-collections-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        ArtistRepository sut = CreateArtistRepository(context);

        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        ArtistContributorEntity keptArtistContributor = _artistContributorEntityFixture.Create(artistId: artist.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals);
        artist.Contributors = [keptArtistContributor, _artistContributorEntityFixture.Create(artistId: artist.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Producer)];

        string keptAlbumTagName = $"remove-album-tag-{Guid.NewGuid():N}";
        string keptAlbumGenreName = $"remove-album-genre-{Guid.NewGuid():N}";
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Tags = [_tagEntityFixture.Create(name: keptAlbumTagName), _tagEntityFixture.Create(name: $"remove-album-tag-two-{Guid.NewGuid():N}")];
        album.Genres = [_genreEntityFixture.Create(name: keptAlbumGenreName), _genreEntityFixture.Create(name: $"remove-album-genre-two-{Guid.NewGuid():N}")];
        album.Ratings =
        [
            _audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100),
            _audioRatingEntityFixture.Create(value: 5, maxValue: 5, source: AudioRatingSource.User, voteCount: 10)
        ];
        AlbumContributorEntity keptAlbumContributor = _albumContributorEntityFixture.Create(albumId: album.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Producer);
        album.Contributors = [keptAlbumContributor, _albumContributorEntityFixture.Create(albumId: album.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Composer)];

        string keptTrackTagName = $"remove-track-tag-{Guid.NewGuid():N}";
        string keptTrackGenreName = $"remove-track-genre-{Guid.NewGuid():N}";
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: keptTrackTagName), _tagEntityFixture.Create(name: $"remove-track-tag-two-{Guid.NewGuid():N}")];
        track.Genres = [_genreEntityFixture.Create(name: keptTrackGenreName), _genreEntityFixture.Create(name: $"remove-track-genre-two-{Guid.NewGuid():N}")];
        track.Ratings =
        [
            _audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100),
            _audioRatingEntityFixture.Create(value: 3, maxValue: 5, source: AudioRatingSource.User, voteCount: 20)
        ];
        track.Moods = [_trackMoodEntityFixture.Create(name: "Calm"), _trackMoodEntityFixture.Create(name: "Upbeat")];
        track.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678"), _trackIsrcEntityFixture.Create(value: "GBAYE0000001")];
        TrackContributorEntity keptTrackContributor = _trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals);
        track.Contributors = [keptTrackContributor, _trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Producer)];
        album.Tracks = [track];
        artist.Albums = [album];
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        // Detach the seeded graph, so that the update must load the tracked aggregate through its include chain instead of returning the already populated seeded instances.
        context.ChangeTracker.Clear();

        ArtistEntity incoming = await LoadDetachedArtistAsync(context, artist.Id);
        AlbumEntity incomingAlbum = Assert.Single(incoming.Albums);
        TrackEntity incomingTrack = Assert.Single(incomingAlbum.Tracks);
        incoming.Contributors = [incoming.Contributors.Single(contributor => contributor.Id == keptArtistContributor.Id)];
        incomingAlbum.Tags = [incomingAlbum.Tags.Single(tag => tag.Name == keptAlbumTagName)];
        incomingAlbum.Genres = [incomingAlbum.Genres.Single(genre => genre.Name == keptAlbumGenreName)];
        incomingAlbum.Ratings = [incomingAlbum.Ratings.Single(rating => rating.Source == AudioRatingSource.MusicBrainz)];
        incomingAlbum.Contributors = [incomingAlbum.Contributors.Single(contributor => contributor.Id == keptAlbumContributor.Id)];
        incomingTrack.Tags = [incomingTrack.Tags.Single(tag => tag.Name == keptTrackTagName)];
        incomingTrack.Genres = [incomingTrack.Genres.Single(genre => genre.Name == keptTrackGenreName)];
        incomingTrack.Ratings = [incomingTrack.Ratings.Single(rating => rating.Source == AudioRatingSource.MusicBrainz)];
        incomingTrack.Moods = [incomingTrack.Moods.Single(mood => mood.Name == "Calm")];
        incomingTrack.Isrcs = [incomingTrack.Isrcs.Single(isrc => isrc.Value == "USRC12345678")];
        incomingTrack.Contributors = [incomingTrack.Contributors.Single(contributor => contributor.Id == keptTrackContributor.Id)];

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        ArtistEntity? storedArtist = await context.Artists.AsNoTracking().Include(candidate => candidate.Contributors).FirstOrDefaultAsync(candidate => candidate.Id == artist.Id);
        Assert.NotNull(storedArtist);
        ArtistContributorEntity storedArtistContributor = Assert.Single(storedArtist!.Contributors);
        Assert.Equal(keptArtistContributor.Id, storedArtistContributor.Id);
        AlbumEntity? storedAlbum = await context.Albums
            .AsNoTracking()
            .Include(candidate => candidate.Tags)
            .Include(candidate => candidate.Genres)
            .Include(candidate => candidate.Ratings)
            .Include(candidate => candidate.Contributors)
            .FirstOrDefaultAsync(candidate => candidate.Id == album.Id);
        Assert.NotNull(storedAlbum);
        Assert.Equal(keptAlbumTagName, Assert.Single(storedAlbum!.Tags).Name);
        Assert.Equal(keptAlbumGenreName, Assert.Single(storedAlbum.Genres).Name);
        Assert.Equal(AudioRatingSource.MusicBrainz, Assert.Single(storedAlbum.Ratings).Source);
        Assert.Equal(keptAlbumContributor.Id, Assert.Single(storedAlbum.Contributors).Id);
        TrackEntity? storedTrack = await context.Tracks
            .AsNoTracking()
            .Include(candidate => candidate.Tags)
            .Include(candidate => candidate.Genres)
            .Include(candidate => candidate.Ratings)
            .Include(candidate => candidate.Moods)
            .Include(candidate => candidate.Isrcs)
            .Include(candidate => candidate.Contributors)
            .FirstOrDefaultAsync(candidate => candidate.Id == track.Id);
        Assert.NotNull(storedTrack);
        Assert.Equal(keptTrackTagName, Assert.Single(storedTrack!.Tags).Name);
        Assert.Equal(keptTrackGenreName, Assert.Single(storedTrack.Genres).Name);
        Assert.Equal(AudioRatingSource.MusicBrainz, Assert.Single(storedTrack.Ratings).Source);
        Assert.Equal("Calm", Assert.Single(storedTrack.Moods).Name);
        Assert.Equal("USRC12345678", Assert.Single(storedTrack.Isrcs).Value);
        Assert.Equal(keptTrackContributor.Id, Assert.Single(storedTrack.Contributors).Id);
    }

    /// <summary>
    /// Seeds an artist that owns the requested number of albums, each with the requested number of tracks, all of them with distinct shared reference names.
    /// </summary>
    /// <param name="context">The context the artist is persisted through.</param>
    /// <param name="libraryId">The Id of the library that owns the artist.</param>
    /// <param name="albumCount">The number of albums the artist owns.</param>
    /// <param name="tracksPerAlbum">The number of tracks each album owns.</param>
    /// <returns>The created artist.</returns>
    private ArtistEntity SeedArtistWithAlbums(LuminaDbContext context, Guid libraryId, int albumCount, int tracksPerAlbum)
    {
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        List<AlbumEntity> albums = [];
        for (int albumIndex = 0; albumIndex < albumCount; albumIndex++)
        {
            AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
            album.Tags = [_tagEntityFixture.Create(name: $"album-tag-{libraryId:N}-{albumIndex}")];
            album.Genres = [_genreEntityFixture.Create(name: $"album-genre-{libraryId:N}-{albumIndex}")];
            List<TrackEntity> tracks = [];
            for (int trackIndex = 0; trackIndex < tracksPerAlbum; trackIndex++)
            {
                TrackEntity track = _trackEntityFixture.Create(
                    albumId: album.Id, libraryId: libraryId, path: $"/music/{libraryId:N}/track-{albumIndex}-{trackIndex}.flac", includeMetadata: false);
                track.Tags = [_tagEntityFixture.Create(name: $"track-tag-{libraryId:N}-{albumIndex}-{trackIndex}")];
                track.Genres = [_genreEntityFixture.Create(name: $"track-genre-{libraryId:N}-{albumIndex}-{trackIndex}")];
                tracks.Add(track);
            }
            album.Tracks = tracks;
            albums.Add(album);
        }
        artist.Albums = albums;
        context.Artists.Add(artist);
        return artist;
    }

    /// <summary>
    /// Creates an artist repository backed by the provided context, wiring the real album and track repositories.
    /// </summary>
    /// <param name="context">The context the repository operates on.</param>
    /// <returns>The created repository.</returns>
    private static ArtistRepository CreateArtistRepository(LuminaDbContext context)
    {
        TrackRepository trackRepository = new(context);
        AlbumRepository albumRepository = new(context, trackRepository);
        return new ArtistRepository(context, albumRepository, trackRepository);
    }

    /// <summary>
    /// Seeds an artist with no albums or contributors.
    /// </summary>
    /// <param name="context">The context the artist is persisted through.</param>
    /// <param name="libraryId">The Id of the library that owns the artist.</param>
    /// <param name="name">The name of the artist.</param>
    /// <returns>The persisted artist.</returns>
    private async Task<ArtistEntity> SeedArtistAsync(LuminaDbContext context, Guid libraryId, string name)
    {
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, name: name, includeAlbums: false, includeContributors: false);
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        return artist;
    }

    /// <summary>
    /// Seeds an artist with one album and one track, including every related collection deterministically.
    /// </summary>
    /// <param name="context">The context the artist is persisted through.</param>
    /// <param name="libraryId">The Id of the library that owns the artist.</param>
    /// <returns>The persisted artist.</returns>
    private async Task<ArtistEntity> SeedFullArtistAsync(LuminaDbContext context, Guid libraryId)
    {
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        artist.Contributors =
            [_artistContributorEntityFixture.Create(artistId: artist.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)];
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
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
        artist.Albums = [album];
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
    /// <returns>The persisted album.</returns>
    private async Task<AlbumEntity> SeedAlbumAsync(LuminaDbContext context, Guid libraryId, Guid artistId)
    {
        AlbumEntity album = _albumEntityFixture.Create(artistId: artistId, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        return album;
    }

    /// <summary>
    /// Loads a detached copy of a stored artist with the whole aggregate, so that it can be handed to the repository as the desired state of an edit.
    /// </summary>
    /// <param name="context">The context that tracks the stored artist.</param>
    /// <param name="artistId">The Id of the artist to load.</param>
    /// <returns>The detached copy of the artist.</returns>
    private static async Task<ArtistEntity> LoadDetachedArtistAsync(LuminaDbContext context, Guid artistId)
    {
        return await context.Artists
            .AsNoTracking()
            .Include(artist => artist.Contributors)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Ratings)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tags)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Genres)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Contributors)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Ratings)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Genres)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Tags)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Moods)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Isrcs)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Contributors)
            .AsSplitQuery()
            .FirstAsync(artist => artist.Id == artistId);
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
    /// Counts the number of read commands issued through a context, so that tests can assert that the number of queries does not scale with the aggregate size.
    /// </summary>
    private sealed class SelectCountingInterceptor : DbCommandInterceptor
    {
        /// <summary>
        /// Gets the number of SELECT commands observed since the last reset.
        /// </summary>
        public int SelectCount { get; private set; }

        /// <summary>
        /// Resets the observed command count.
        /// </summary>
        public void Reset()
        {
            SelectCount = 0;
        }

        /// <summary>
        /// Counts the command before it is executed synchronously.
        /// </summary>
        /// <param name="command">The command that is about to be executed.</param>
        /// <param name="eventData">The event data of the command.</param>
        /// <param name="result">The interception result.</param>
        /// <returns>The interception result, unmodified.</returns>
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            CountIfSelect(command);
            return base.ReaderExecuting(command, eventData, result);
        }

        /// <summary>
        /// Counts the command before it is executed asynchronously.
        /// </summary>
        /// <param name="command">The command that is about to be executed.</param>
        /// <param name="eventData">The event data of the command.</param>
        /// <param name="result">The interception result.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
        /// <returns>The interception result, unmodified.</returns>
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            CountIfSelect(command);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        /// <summary>
        /// Increments the count when <paramref name="command"/> is a SELECT statement.
        /// </summary>
        /// <param name="command">The command to inspect.</param>
        private void CountIfSelect(DbCommand command)
        {
            if (command.CommandText.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                SelectCount++;
        }
    }
}
