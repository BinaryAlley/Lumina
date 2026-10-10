#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DTO.Filtering;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.DataAccess.Core.Repositories.MusicLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
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
/// Contains unit tests for the <see cref="ArtistRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly AlbumRepository _albumRepository;
    private readonly TrackRepository _trackRepository;
    private readonly ArtistRepository _sut;
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
    private readonly BaseFilterDtoFixture _baseFilterDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtistRepositoryTests"/> class.
    /// </summary>
    public ArtistRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _trackRepository = new TrackRepository(_mockContext);
        _albumRepository = new AlbumRepository(_mockContext, _trackRepository);
        _sut = new ArtistRepository(_mockContext, _albumRepository, _trackRepository);
    }

    [Fact]
    public async Task InsertAsync_WhenArtistDoesNotExist_ShouldAddArtistToContextAndReturnCreated()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);

        // Act
        Result<Created> result = await _sut.InsertAsync(artist, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<ArtistEntity>? addedArtist = _mockContext.ChangeTracker.Entries<ArtistEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == artist.Id);
        Assert.NotNull(addedArtist);
    }

    [Fact]
    public async Task InsertAsync_WhenArtistWithTheSameIdAlreadyExists_ShouldReturnArtistAlreadyExists()
    {
        // Arrange
        ArtistEntity existingArtist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        _mockContext.Artists.Add(existingArtist);
        await _mockContext.SaveChangesAsync();

        ArtistEntity artist = _artistEntityFixture.Create(id: existingArtist.Id, includeAlbums: false, includeContributors: false);

        // Act
        Result<Created> result = await _sut.InsertAsync(artist, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistAlreadyExists, result.FirstError);
        Assert.Single(_mockContext.ChangeTracker.Entries<ArtistEntity>()); // Only the existing artist should remain tracked.
    }

    [Fact]
    public async Task InsertAsync_WhenAnotherArtistOfTheSameLibraryHasTheSameName_ShouldReturnArtistAlreadyExists()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity existingArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Queen", includeAlbums: false, includeContributors: false);
        _mockContext.Artists.Add(existingArtist);
        await _mockContext.SaveChangesAsync();

        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, name: "Queen", includeAlbums: false, includeContributors: false);

        // Act
        Result<Created> result = await _sut.InsertAsync(artist, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task InsertAsync_WhenAnotherLibraryHasAnArtistWithTheSameName_ShouldAddArtistToContextAndReturnCreated()
    {
        // Arrange
        ArtistEntity existingArtist = _artistEntityFixture.Create(libraryId: Guid.NewGuid(), name: "Queen", includeAlbums: false, includeContributors: false);
        _mockContext.Artists.Add(existingArtist);
        await _mockContext.SaveChangesAsync();

        ArtistEntity artist = _artistEntityFixture.Create(libraryId: Guid.NewGuid(), name: "Queen", includeAlbums: false, includeContributors: false);

        // Act
        Result<Created> result = await _sut.InsertAsync(artist, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        // An artist only has to be unique within its own library, so the second library is allowed to register the same name.
        Assert.Contains(_mockContext.ChangeTracker.Entries<ArtistEntity>(), entry => entry.State == EntityState.Added && entry.Entity.Id == artist.Id);
    }

    [Fact]
    public async Task InsertAsync_WhenTwoTracksOfTheSameRequestShareAPath_ShouldReturnTrackAlreadyExists()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: artist.LibraryId, includeTracks: false, includeMetadata: false);
        TrackEntity firstTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: artist.LibraryId, path: "/music/queen/duplicate.flac", includeMetadata: false);
        TrackEntity secondTrack = _trackEntityFixture.Create(albumId: album.Id, libraryId: artist.LibraryId, path: "/music/queen/duplicate.flac", includeMetadata: false);
        album.Tracks = [firstTrack, secondTrack];
        artist.Albums = [album];

        // Act
        Result<Created> result = await _sut.InsertAsync(artist, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackAlreadyExists, result.FirstError);
        // The duplicate within the request is detected before anything is written.
        Assert.Empty(_mockContext.ChangeTracker.Entries<ArtistEntity>());
    }

    [Fact]
    public async Task InsertAsync_WhenATrackPathIsAlreadyStoredInTheLibrary_ShouldReturnTrackAlreadyExists()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        AlbumEntity storedAlbum = _albumEntityFixture.Create(artistId: Guid.NewGuid(), libraryId: libraryId, includeTracks: false, includeMetadata: false);
        storedAlbum.Tracks = [_trackEntityFixture.Create(albumId: storedAlbum.Id, libraryId: libraryId, path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false)];
        _mockContext.Albums.Add(storedAlbum);
        await _mockContext.SaveChangesAsync();

        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Tracks = [_trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false)];
        artist.Albums = [album];

        // Act
        Result<Created> result = await _sut.InsertAsync(artist, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackAlreadyExists, result.FirstError);
        Assert.Empty(_mockContext.ChangeTracker.Entries<ArtistEntity>());
    }

    [Fact]
    public async Task InsertAsync_WhenAnotherLibraryHasATrackWithTheSamePath_ShouldAddArtistToContextAndReturnCreated()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: artist.LibraryId, includeTracks: false, includeMetadata: false);
        album.Tracks = [_trackEntityFixture.Create(albumId: album.Id, libraryId: artist.LibraryId, path: "/music/queen/we-will-rock-you.flac", includeMetadata: false)];
        artist.Albums = [album];

        // Act
        Result<Created> result = await _sut.InsertAsync(artist, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        // The same physical path is only unique within its library, so another library is allowed to register it.
        Assert.Contains(_mockContext.ChangeTracker.Entries<ArtistEntity>(), entry => entry.State == EntityState.Added && entry.Entity.Id == artist.Id);
    }

    [Fact]
    public async Task InsertAsync_WhenExistingTagAndGenreNamesAreFound_ShouldReplaceThemWithTheStoredInstancesAcrossAlbumsAndTracks()
    {
        // Arrange
        TagEntity existingTag = _tagEntityFixture.Create(name: "ExistingTag");
        GenreEntity existingGenre = _genreEntityFixture.Create(name: "ExistingGenre");
        _mockContext.Set<TagEntity>().Add(existingTag);
        _mockContext.Set<GenreEntity>().Add(existingGenre);
        await _mockContext.SaveChangesAsync();

        ArtistEntity artist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: artist.LibraryId, includeTracks: false, includeMetadata: false);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: artist.LibraryId, includeMetadata: false);
        album.Tags = [_tagEntityFixture.Create(name: "ExistingTag"), _tagEntityFixture.Create(name: "NewAlbumTag")];
        album.Genres = [_genreEntityFixture.Create(name: "ExistingGenre"), _genreEntityFixture.Create(name: "NewAlbumGenre")];
        track.Tags = [_tagEntityFixture.Create(name: "ExistingTag"), _tagEntityFixture.Create(name: "NewTrackTag")];
        track.Genres = [_genreEntityFixture.Create(name: "ExistingGenre"), _genreEntityFixture.Create(name: "NewTrackGenre")];
        album.Tracks = [track];
        artist.Albums = [album];

        // Act
        Result<Created> result = await _sut.InsertAsync(artist, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<ArtistEntity>? addedArtist = _mockContext.ChangeTracker.Entries<ArtistEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == artist.Id);
        Assert.NotNull(addedArtist);
        AlbumEntity addedAlbum = Assert.Single(addedArtist!.Entity.Albums);
        Assert.Contains(addedAlbum.Tags, tag => tag.Name == "ExistingTag" && ReferenceEquals(tag, existingTag));
        Assert.Contains(addedAlbum.Tags, tag => tag.Name == "NewAlbumTag" && !ReferenceEquals(tag, existingTag));
        Assert.Contains(addedAlbum.Genres, genre => genre.Name == "ExistingGenre" && ReferenceEquals(genre, existingGenre));
        Assert.Contains(addedAlbum.Genres, genre => genre.Name == "NewAlbumGenre" && !ReferenceEquals(genre, existingGenre));
        TrackEntity addedTrack = Assert.Single(addedAlbum.Tracks);
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
        Result<Created> result = await _sut.InsertAsync(artist, CancellationToken.None);

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
    public async Task InsertAsync_WhenArtistHasNoTagsOrGenres_ShouldStillAddArtistAndReturnCreated()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(includeAlbums: true, includeContributors: false, includeMetadata: false);
        AlbumEntity album = Assert.Single(artist.Albums);
        album.Tags = [];
        album.Genres = [];
        album.Tracks[0].Tags = [];
        album.Tracks[0].Genres = [];

        // Act
        Result<Created> result = await _sut.InsertAsync(artist, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<ArtistEntity>? addedArtist = _mockContext.ChangeTracker.Entries<ArtistEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == artist.Id);
        Assert.NotNull(addedArtist);
        Assert.Empty(addedArtist!.Entity.Albums[0].Tags);
        Assert.Empty(addedArtist.Entity.Albums[0].Genres);
    }

    [Fact]
    public async Task UpdateAsync_WhenArtistDoesNotExist_ShouldReturnArtistNotFound()
    {
        // Arrange
        ArtistEntity data = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNotFound, result.FirstError);
    }

    [Fact]
    public async Task UpdateAsync_WhenArtistExists_ShouldUpdateItsScalarProperties()
    {
        // Arrange
        ArtistEntity storedArtist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        _mockContext.Artists.Add(storedArtist);
        await _mockContext.SaveChangesAsync();

        ArtistEntity data = _artistEntityFixture.Create(id: storedArtist.Id, includeAlbums: false, includeContributors: false);
        data.Name = "Freddie Mercury";
        data.Website = "https://www.queenonline.com";

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        ArtistEntity? retrievedArtist = await _mockContext.Artists.FindAsync(storedArtist.Id);
        Assert.NotNull(retrievedArtist);
        Assert.Equal("Freddie Mercury", retrievedArtist!.Name);
        Assert.Equal("https://www.queenonline.com", retrievedArtist.Website);
    }

    [Fact]
    public async Task UpdateAsync_WhenIncomingLibraryIdDiffers_ShouldPreserveTheStoredLibraryId()
    {
        // Arrange
        Guid storedLibraryId = Guid.NewGuid();
        ArtistEntity storedArtist = _artistEntityFixture.Create(libraryId: storedLibraryId, includeAlbums: false, includeContributors: false);
        _mockContext.Artists.Add(storedArtist);
        await _mockContext.SaveChangesAsync();

        ArtistEntity data = _artistEntityFixture.Create(id: storedArtist.Id, includeAlbums: false, includeContributors: false);
        data.LibraryId = Guid.NewGuid();
        data.Name = "Queen";

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        ArtistEntity? retrievedArtist = await _mockContext.Artists.FindAsync(storedArtist.Id);
        Assert.NotNull(retrievedArtist);
        Assert.Equal(storedLibraryId, retrievedArtist!.LibraryId);
    }

    [Fact]
    public async Task UpdateAsync_WhenIncomingAuditColumnsDiffer_ShouldPreserveTheStoredAuditColumns()
    {
        // Arrange
        DateTime storedCreatedOnUtc = new(2020, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        Guid storedCreatedBy = Guid.NewGuid();
        ArtistEntity storedArtist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        storedArtist.CreatedOnUtc = storedCreatedOnUtc;
        storedArtist.CreatedBy = storedCreatedBy;
        storedArtist.UpdatedOnUtc = null;
        storedArtist.UpdatedBy = null;
        _mockContext.Artists.Add(storedArtist);
        await _mockContext.SaveChangesAsync();

        ArtistEntity data = _artistEntityFixture.Create(id: storedArtist.Id, includeAlbums: false, includeContributors: false);
        data.Name = "Freddie";
        data.CreatedOnUtc = DateTime.UtcNow.AddYears(-10);
        data.CreatedBy = Guid.NewGuid();
        data.UpdatedOnUtc = DateTime.UtcNow;
        data.UpdatedBy = Guid.NewGuid();

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        ArtistEntity? retrievedArtist = await _mockContext.Artists.FindAsync(storedArtist.Id);
        Assert.NotNull(retrievedArtist);
        Assert.Equal("Freddie", retrievedArtist!.Name);
        // The audit columns are owned by the auditing interceptor alone, so an edit never overwrites them.
        Assert.Equal(storedCreatedOnUtc, retrievedArtist.CreatedOnUtc);
        Assert.Equal(storedCreatedBy, retrievedArtist.CreatedBy);
        Assert.Null(retrievedArtist.UpdatedOnUtc);
        Assert.Null(retrievedArtist.UpdatedBy);
    }

    [Fact]
    public async Task UpdateAsync_WhenAnUnchangedContributorIsSubmitted_ShouldPreserveItsStoredIdentityAndAuditColumns()
    {
        // Arrange
        Guid keptMediaContributorId = Guid.NewGuid();
        ArtistEntity storedArtist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        ArtistContributorEntity keptContributor = _artistContributorEntityFixture.Create(
            artistId: storedArtist.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Producer);
        storedArtist.Contributors = [keptContributor];
        _mockContext.Artists.Add(storedArtist);
        await _mockContext.SaveChangesAsync();

        Guid keptContributorId = keptContributor.Id;
        DateTime keptContributorCreatedOnUtc = keptContributor.CreatedOnUtc;

        ArtistEntity data = _artistEntityFixture.Create(id: storedArtist.Id, includeAlbums: false, includeContributors: false);
        data.Contributors =
            [_artistContributorEntityFixture.Create(artistId: storedArtist.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Producer)];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        ArtistContributorEntity storedContributor = Assert.Single(
            await _mockContext.Set<ArtistContributorEntity>().Where(contributor => contributor.ArtistId == storedArtist.Id).ToListAsync());
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
        ArtistEntity storedArtist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        ArtistContributorEntity keptContributor = _artistContributorEntityFixture.Create(
            artistId: storedArtist.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Producer);
        ArtistContributorEntity removedContributor = _artistContributorEntityFixture.Create(
            artistId: storedArtist.Id, mediaContributorId: removedMediaContributorId, role: MediaContributorRole.Composer);
        storedArtist.Contributors = [keptContributor, removedContributor];
        _mockContext.Artists.Add(storedArtist);
        await _mockContext.SaveChangesAsync();

        Guid keptContributorId = keptContributor.Id;

        ArtistEntity data = _artistEntityFixture.Create(id: storedArtist.Id, includeAlbums: false, includeContributors: false);
        data.Contributors =
        [
            _artistContributorEntityFixture.Create(artistId: storedArtist.Id, mediaContributorId: keptMediaContributorId, role: MediaContributorRole.Producer),
            _artistContributorEntityFixture.Create(artistId: storedArtist.Id, mediaContributorId: addedMediaContributorId, role: MediaContributorRole.Vocals)
        ];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        List<ArtistContributorEntity> storedContributors = await _mockContext.Set<ArtistContributorEntity>()
            .Where(contributor => contributor.ArtistId == storedArtist.Id)
            .ToListAsync();
        Assert.Equal(2, storedContributors.Count);
        // The unchanged participation keeps its identity, while only the delta is deleted and inserted.
        Assert.Equal(keptContributorId, storedContributors.Single(contributor => contributor.MediaContributorId == keptMediaContributorId).Id);
        Assert.DoesNotContain(storedContributors, contributor => contributor.MediaContributorId == removedMediaContributorId);
        Assert.Contains(storedContributors, contributor => contributor.MediaContributorId == addedMediaContributorId);
    }

    [Fact]
    public async Task UpdateAsync_WhenAlbumsChange_ShouldInsertTheAddedUpdateTheExistingAndRemoveTheMissingOnes()
    {
        // Arrange
        ArtistEntity storedArtist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        AlbumEntity updatedAlbum = _albumEntityFixture.Create(artistId: storedArtist.Id, libraryId: storedArtist.LibraryId, includeTracks: false, includeMetadata: false);
        AlbumEntity removedAlbum = _albumEntityFixture.Create(artistId: storedArtist.Id, libraryId: storedArtist.LibraryId, includeTracks: false, includeMetadata: false);
        storedArtist.Albums = [updatedAlbum, removedAlbum];
        _mockContext.Artists.Add(storedArtist);
        await _mockContext.SaveChangesAsync();

        AlbumEntity incomingUpdatedAlbum = _albumEntityFixture.Create(id: updatedAlbum.Id, artistId: storedArtist.Id, libraryId: storedArtist.LibraryId, includeTracks: false, includeMetadata: false);
        AlbumEntity incomingAddedAlbum = _albumEntityFixture.Create(artistId: storedArtist.Id, libraryId: storedArtist.LibraryId, includeTracks: false, includeMetadata: false);
        ArtistEntity data = _artistEntityFixture.Create(id: storedArtist.Id, includeAlbums: false, includeContributors: false);
        data.Albums = [incomingUpdatedAlbum, incomingAddedAlbum];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        List<AlbumEntity> storedAlbums = await _mockContext.Albums.ToListAsync();
        Assert.Contains(storedAlbums, album => album.Id == updatedAlbum.Id);
        Assert.Contains(storedAlbums, album => album.Id == incomingAddedAlbum.Id);
        Assert.DoesNotContain(storedAlbums, album => album.Id == removedAlbum.Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenTwoNewTracksOfTheRequestShareAPath_ShouldReturnTrackAlreadyExistsBeforeWriting()
    {
        // Arrange
        ArtistEntity storedArtist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        AlbumEntity storedAlbum = _albumEntityFixture.Create(artistId: storedArtist.Id, libraryId: storedArtist.LibraryId, includeTracks: false, includeMetadata: false);
        storedAlbum.Tracks = [_trackEntityFixture.Create(albumId: storedAlbum.Id, libraryId: storedArtist.LibraryId, includeMetadata: false)];
        storedArtist.Albums = [storedAlbum];
        _mockContext.Artists.Add(storedArtist);
        await _mockContext.SaveChangesAsync();

        AlbumEntity incomingAlbum = _albumEntityFixture.Create(id: storedAlbum.Id, artistId: storedArtist.Id, libraryId: storedArtist.LibraryId, includeTracks: false, includeMetadata: false);
        incomingAlbum.Tracks =
        [
            _trackEntityFixture.Create(albumId: storedAlbum.Id, libraryId: storedArtist.LibraryId, path: "/music/queen/duplicate.flac", includeMetadata: false),
            _trackEntityFixture.Create(albumId: storedAlbum.Id, libraryId: storedArtist.LibraryId, path: "/music/queen/duplicate.flac", includeMetadata: false)
        ];
        ArtistEntity data = _artistEntityFixture.Create(id: storedArtist.Id, includeAlbums: false, includeContributors: false);
        data.Albums = [incomingAlbum];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackAlreadyExists, result.FirstError);
        // The unique path is checked for the whole request before any album or track is written.
        Assert.DoesNotContain(_mockContext.ChangeTracker.Entries<AlbumEntity>(), entry => entry.State == EntityState.Added);
        Assert.DoesNotContain(_mockContext.ChangeTracker.Entries<TrackEntity>(), entry => entry.State == EntityState.Added);
    }

    [Fact]
    public async Task UpdateAsync_WhenANewTrackPathIsAlreadyStoredInTheLibrary_ShouldReturnTrackAlreadyExistsBeforeWriting()
    {
        // Arrange
        ArtistEntity storedArtist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        AlbumEntity storedAlbum = _albumEntityFixture.Create(artistId: storedArtist.Id, libraryId: storedArtist.LibraryId, includeTracks: false, includeMetadata: false);
        storedAlbum.Tracks = [_trackEntityFixture.Create(albumId: storedAlbum.Id, libraryId: storedArtist.LibraryId, path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false)];
        storedArtist.Albums = [storedAlbum];
        _mockContext.Artists.Add(storedArtist);
        await _mockContext.SaveChangesAsync();

        AlbumEntity incomingAlbum = _albumEntityFixture.Create(id: storedAlbum.Id, artistId: storedArtist.Id, libraryId: storedArtist.LibraryId, includeTracks: false, includeMetadata: false);
        incomingAlbum.Tracks = [_trackEntityFixture.Create(albumId: storedAlbum.Id, libraryId: storedArtist.LibraryId, path: "/music/queen/bohemian-rhapsody.flac", includeMetadata: false)];
        ArtistEntity data = _artistEntityFixture.Create(id: storedArtist.Id, includeAlbums: false, includeContributors: false);
        data.Albums = [incomingAlbum];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackAlreadyExists, result.FirstError);
        Assert.DoesNotContain(_mockContext.ChangeTracker.Entries<AlbumEntity>(), entry => entry.State == EntityState.Added);
        Assert.DoesNotContain(_mockContext.ChangeTracker.Entries<TrackEntity>(), entry => entry.State == EntityState.Added);
    }

    [Fact]
    public async Task UpdateAsync_WhenTracksChangeWithinAnAlbum_ShouldInsertTheAddedUpdateTheExistingAndRemoveTheMissingOnes()
    {
        // Arrange
        ArtistEntity storedArtist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        AlbumEntity storedAlbum = _albumEntityFixture.Create(artistId: storedArtist.Id, libraryId: storedArtist.LibraryId, includeTracks: false, includeMetadata: false);
        TrackEntity keptTrack = _trackEntityFixture.Create(albumId: storedAlbum.Id, libraryId: storedArtist.LibraryId, includeMetadata: false);
        TrackEntity removedTrack = _trackEntityFixture.Create(albumId: storedAlbum.Id, libraryId: storedArtist.LibraryId, includeMetadata: false);
        storedAlbum.Tracks = [keptTrack, removedTrack];
        storedArtist.Albums = [storedAlbum];
        _mockContext.Artists.Add(storedArtist);
        await _mockContext.SaveChangesAsync();

        TrackEntity incomingKeptTrack = _trackEntityFixture.Create(id: keptTrack.Id, albumId: storedAlbum.Id, libraryId: storedArtist.LibraryId, includeMetadata: false);
        incomingKeptTrack.Title = "Somebody to Love";
        TrackEntity incomingAddedTrack = _trackEntityFixture.Create(albumId: storedAlbum.Id, libraryId: storedArtist.LibraryId, includeMetadata: false);
        AlbumEntity incomingAlbum = _albumEntityFixture.Create(id: storedAlbum.Id, artistId: storedArtist.Id, libraryId: storedArtist.LibraryId, includeTracks: false, includeMetadata: false);
        incomingAlbum.Tracks = [incomingKeptTrack, incomingAddedTrack];
        ArtistEntity data = _artistEntityFixture.Create(id: storedArtist.Id, includeAlbums: false, includeContributors: false);
        data.Albums = [incomingAlbum];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        List<TrackEntity> storedTracks = await _mockContext.Tracks.ToListAsync();
        Assert.Contains(storedTracks, track => track.Id == keptTrack.Id);
        Assert.Contains(storedTracks, track => track.Id == incomingAddedTrack.Id);
        Assert.DoesNotContain(storedTracks, track => track.Id == removedTrack.Id);
        Assert.Equal("Somebody to Love", storedTracks.Single(track => track.Id == keptTrack.Id).Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenArtistExists_ShouldReturnTheArtist()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        _mockContext.Artists.Add(artist);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<ArtistEntity?> result = await _sut.GetByIdAsync(artist.Id, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(artist.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenArtistDoesNotExist_ShouldReturnNull()
    {
        // Act
        Result<ArtistEntity?> result = await _sut.GetByIdAsync(Guid.NewGuid(), cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNavigationPropertiesAreIncluded_ShouldPopulateTheRelatedCollections()
    {
        // Arrange
        ArtistEntity artist = CreateArtistWithFullAggregate();
        _mockContext.Artists.Add(artist);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<ArtistEntity?> result = await _sut.GetByIdAsync(artist.Id, cancellationToken: CancellationToken.None);

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
        Assert.Equal(2, retrievedTrack.Tags.Count);
        Assert.Equal(2, retrievedTrack.Genres.Count);
        Assert.Equal(2, retrievedTrack.Moods.Count);
        Assert.Equal(2, retrievedTrack.Isrcs.Count);
        Assert.Single(retrievedTrack.Contributors);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenArtistExists_ShouldRemoveItAndReturnDeleted()
    {
        // Arrange
        ArtistEntity artist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        _mockContext.Artists.Add(artist);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByIdAsync(artist.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        Assert.Contains(_mockContext.ChangeTracker.Entries<ArtistEntity>(), entry => entry.State == EntityState.Deleted && entry.Entity.Id == artist.Id);
        await _mockContext.SaveChangesAsync();
        Assert.DoesNotContain(await _mockContext.Artists.ToListAsync(), candidate => candidate.Id == artist.Id);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenArtistDoesNotExist_ShouldReturnArtistNotFound()
    {
        // Act
        Result<Deleted> result = await _sut.DeleteByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNotFound, result.FirstError);
    }

    [Fact]
    public async Task GetAllAsync_WhenFilterIsNotALibraryFilter_ShouldReturnFilterMustIncludeLibraryId()
    {
        // Arrange
        BaseFilterDto filter = _baseFilterDtoFixture.Create();

        // Act
        Result<PaginatedResultDto<ArtistEntity>> result = await _sut.GetAllAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.FilterMustIncludeLibraryId, result.FirstError);
    }

    [Fact]
    public async Task GetAllAsync_WhenFilterLibraryIdIsEmpty_ShouldReturnFilterMustIncludeLibraryId()
    {
        // Arrange
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: Guid.Empty);

        // Act
        Result<PaginatedResultDto<ArtistEntity>> result = await _sut.GetAllAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.FilterMustIncludeLibraryId, result.FirstError);
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldReturnOnlyTheArtistsOfTheLibrary()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity firstArtist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        ArtistEntity secondArtist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        ArtistEntity artistOfAnotherLibrary = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        _mockContext.Artists.AddRange(firstArtist, secondArtist, artistOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistEntity>> result = await _sut.GetAllAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value.Data, artist => Assert.Equal(libraryId, artist.LibraryId));
        Assert.DoesNotContain(result.Value.Data, artist => artist.Id == artistOfAnotherLibrary.Id);
    }

    [Theory]
    [InlineData("Queens")] // The search term matches the stored casing.
    [InlineData("queens")] // The search term is entirely lower case.
    [InlineData("QUEENS")] // The search term is entirely upper case.
    public async Task GetAllAsync_WhenSearchTermProvided_ShouldMatchTheNameCaseInsensitively(string searchTerm)
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity matchingArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Queens of the Stone Age", includeAlbums: false, includeContributors: false);
        ArtistEntity otherArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Radiohead", includeAlbums: false, includeContributors: false);
        _mockContext.Artists.AddRange(matchingArtist, otherArtist);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId, searchTerm: searchTerm);

        // Act
        Result<PaginatedResultDto<ArtistEntity>> result = await _sut.GetAllAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ArtistEntity retrievedArtist = Assert.Single(result.Value.Data);
        Assert.Equal(matchingArtist.Id, retrievedArtist.Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldOrderByNameThenById()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity betaArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Beta", includeAlbums: false, includeContributors: false);
        ArtistEntity secondAlphaArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Alpha", includeAlbums: false, includeContributors: false);
        ArtistEntity firstAlphaArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Alpha", includeAlbums: false, includeContributors: false);
        _mockContext.Artists.AddRange(betaArtist, secondAlphaArtist, firstAlphaArtist);
        await _mockContext.SaveChangesAsync();

        List<ArtistEntity> seededArtists = [betaArtist, secondAlphaArtist, firstAlphaArtist];
        Guid[] expectedOrder = [.. seededArtists.OrderBy(artist => artist.Name).ThenBy(artist => artist.Id).Select(artist => artist.Id)];
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistEntity>> result = await _sut.GetAllAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedOrder, result.Value.Data.Select(artist => artist.Id));
    }

    [Fact]
    public async Task GetAllAsync_WhenSortOrderIsDescending_ShouldOrderByNameDescendingThenById()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity alphaArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Alpha", includeAlbums: false, includeContributors: false);
        ArtistEntity betaArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Beta", includeAlbums: false, includeContributors: false);
        _mockContext.Artists.AddRange(alphaArtist, betaArtist);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistEntity>> result = await _sut.GetAllAsync(null, null, SortOrder.Descending, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal([betaArtist.Id, alphaArtist.Id], result.Value.Data.Select(artist => artist.Id));
    }

    [Theory]
    [InlineData("website", SortOrder.Ascending)] // the website field, ascending
    [InlineData("website", SortOrder.Descending)] // the website field, descending
    [InlineData("musicbrainzartistid", SortOrder.Ascending)] // the MusicBrainz artist Id field, ascending
    [InlineData("musicbrainzartistid", SortOrder.Descending)] // the MusicBrainz artist Id field, descending
    [InlineData("createdonutc", SortOrder.Ascending)] // the creation timestamp field, ascending
    [InlineData("createdonutc", SortOrder.Descending)] // the creation timestamp field, descending
    [InlineData("updatedonutc", SortOrder.Ascending)] // the update timestamp field, ascending
    [InlineData("updatedonutc", SortOrder.Descending)] // the update timestamp field, descending
    public async Task GetAllAsync_WhenSortedByField_ShouldOrderByFieldThenById(string sortBy, SortOrder sortOrder)
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity secondArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Second", includeAlbums: false, includeContributors: false);
        ArtistEntity firstArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "First", includeAlbums: false, includeContributors: false);
        // The two seeded artists carry a smaller value for every sortable field, so each sort key has a deterministic expected order.
        firstArtist.Website = "https://a.example.com";
        secondArtist.Website = "https://b.example.com";
        firstArtist.MusicBrainzArtistId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        secondArtist.MusicBrainzArtistId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        firstArtist.CreatedOnUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        secondArtist.CreatedOnUtc = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        firstArtist.UpdatedOnUtc = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        secondArtist.UpdatedOnUtc = new DateTime(2024, 2, 2, 0, 0, 0, DateTimeKind.Utc);
        _mockContext.Artists.AddRange(secondArtist, firstArtist);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistEntity>> result = await _sut.GetAllAsync(null, sortBy, sortOrder, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Guid[] expectedOrder = sortOrder == SortOrder.Descending
            ? [secondArtist.Id, firstArtist.Id]
            : [firstArtist.Id, secondArtist.Id];
        Assert.Equal(expectedOrder, result.Value.Data.Select(artist => artist.Id));
    }

    [Fact]
    public async Task GetAllAsync_WhenPaginationDataIsNull_ShouldReturnAllArtistsWithExpectedMetadata()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        List<ArtistEntity> artists = _artistEntityFixture.CreateMany(3);
        foreach (ArtistEntity artist in artists)
            artist.LibraryId = libraryId;
        _mockContext.Artists.AddRange(artists);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistEntity>> result = await _sut.GetAllAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        PaginatedResultDto<ArtistEntity> page = result.Value;
        Assert.Equal(3, page.Count);
        Assert.Equal(3, page.Data.Count);
        Assert.Equal(1, page.CurrentPage);
        Assert.Equal(3, page.PerPage);
        Assert.Equal(1, page.NumberOfPages);
    }

    [Theory]
    [InlineData(1, 2, 1, 2)] // The first page is fully returned.
    [InlineData(2, 2, 2, 2)] // A middle page is fully returned.
    [InlineData(3, 2, 3, 1)] // The last page is partially returned.
    [InlineData(99, 2, 3, 1)] // A page beyond the last one is clamped to the last page.
    public async Task GetAllAsync_WhenPaginationIsProvided_ShouldReturnTheRequestedPage(int requestedPage, int perPage, int expectedCurrentPage, int expectedRowCount)
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        List<ArtistEntity> artists = _artistEntityFixture.CreateMany(5);
        foreach (ArtistEntity artist in artists)
            artist.LibraryId = libraryId;
        _mockContext.Artists.AddRange(artists);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: requestedPage, perPage: perPage);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistEntity>> result = await _sut.GetAllAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        PaginatedResultDto<ArtistEntity> page = result.Value;
        Assert.Equal(5, page.Count);
        Assert.Equal(3, page.NumberOfPages);
        Assert.Equal(expectedCurrentPage, page.CurrentPage);
        Assert.Equal(perPage, page.PerPage);
        Assert.Equal(expectedRowCount, page.Data.Count);
    }

    [Fact]
    public async Task GetAllAsync_WhenNavigationPropertiesAreIncluded_ShouldPopulateTheRelatedCollections()
    {
        // Arrange
        ArtistEntity artist = CreateArtistWithFullAggregate();
        _mockContext.Artists.Add(artist);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: artist.LibraryId);

        // Act
        Result<PaginatedResultDto<ArtistEntity>> result = await _sut.GetAllAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ArtistEntity retrievedArtist = Assert.Single(result.Value.Data);
        Assert.Single(retrievedArtist.Contributors);
        AlbumEntity retrievedAlbum = Assert.Single(retrievedArtist.Albums);
        Assert.Single(retrievedAlbum.Ratings);
        Assert.Equal(2, retrievedAlbum.Tags.Count);
        Assert.Equal(2, retrievedAlbum.Genres.Count);
        Assert.Single(retrievedAlbum.Contributors);
        TrackEntity retrievedTrack = Assert.Single(retrievedAlbum.Tracks);
        Assert.Single(retrievedTrack.Ratings);
        Assert.Equal(2, retrievedTrack.Moods.Count);
        Assert.Single(retrievedTrack.Contributors);
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenFilterIsNotALibraryFilter_ShouldReturnFilterMustIncludeLibraryId()
    {
        // Arrange
        BaseFilterDto filter = _baseFilterDtoFixture.Create();

        // Act
        Result<PaginatedResultDto<ArtistLiteRow>> result = await _sut.GetAllLiteAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.FilterMustIncludeLibraryId, result.FirstError);
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenFilterLibraryIdIsEmpty_ShouldReturnFilterMustIncludeLibraryId()
    {
        // Arrange
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: Guid.Empty);

        // Act
        Result<PaginatedResultDto<ArtistLiteRow>> result = await _sut.GetAllLiteAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.FilterMustIncludeLibraryId, result.FirstError);
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenCalled_ShouldReturnOnlyTheArtistsOfTheLibrary()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artistOfLibrary = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        ArtistEntity artistOfAnotherLibrary = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        _mockContext.Artists.AddRange(artistOfLibrary, artistOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistLiteRow>> result = await _sut.GetAllLiteAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ArtistLiteRow row = Assert.Single(result.Value.Data);
        Assert.Equal(artistOfLibrary.Id, row.Id);
        Assert.Equal(1, result.Value.Count);
    }

    [Theory]
    [InlineData("Queens")] // The search term matches the stored casing.
    [InlineData("queens")] // The search term is entirely lower case.
    [InlineData("QUEENS")] // The search term is entirely upper case.
    public async Task GetAllLiteAsync_WhenSearchTermProvided_ShouldMatchTheNameCaseInsensitively(string searchTerm)
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity matchingArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Queens of the Stone Age", includeAlbums: false, includeContributors: false);
        ArtistEntity otherArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Radiohead", includeAlbums: false, includeContributors: false);
        _mockContext.Artists.AddRange(matchingArtist, otherArtist);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId, searchTerm: searchTerm);

        // Act
        Result<PaginatedResultDto<ArtistLiteRow>> result = await _sut.GetAllLiteAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ArtistLiteRow row = Assert.Single(result.Value.Data);
        Assert.Equal(matchingArtist.Id, row.Id);
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenCalled_ShouldOrderByNameThenById()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity betaArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Beta", includeAlbums: false, includeContributors: false);
        ArtistEntity secondAlphaArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Alpha", includeAlbums: false, includeContributors: false);
        ArtistEntity firstAlphaArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Alpha", includeAlbums: false, includeContributors: false);
        _mockContext.Artists.AddRange(betaArtist, secondAlphaArtist, firstAlphaArtist);
        await _mockContext.SaveChangesAsync();

        List<ArtistEntity> seededArtists = [betaArtist, secondAlphaArtist, firstAlphaArtist];
        Guid[] expectedOrder = [.. seededArtists.OrderBy(artist => artist.Name).ThenBy(artist => artist.Id).Select(artist => artist.Id)];
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistLiteRow>> result = await _sut.GetAllLiteAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedOrder, result.Value.Data.Select(row => row.Id));
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenSortOrderIsDescending_ShouldOrderByNameDescendingThenById()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity alphaArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Alpha", includeAlbums: false, includeContributors: false);
        ArtistEntity betaArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Beta", includeAlbums: false, includeContributors: false);
        _mockContext.Artists.AddRange(alphaArtist, betaArtist);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistLiteRow>> result = await _sut.GetAllLiteAsync(null, null, SortOrder.Descending, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal([betaArtist.Id, alphaArtist.Id], result.Value.Data.Select(artist => artist.Id));
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenCalled_ShouldProjectOnlyTheIdAndTheName()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, name: "Queen", includeAlbums: false, includeContributors: false);
        _mockContext.Artists.Add(artist);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistLiteRow>> result = await _sut.GetAllLiteAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ArtistLiteRow row = Assert.Single(result.Value.Data);
        Assert.Equal(artist.Id, row.Id);
        Assert.Equal("Queen", row.Name);
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenPaginationDataIsNull_ShouldReturnAllArtistsWithExpectedMetadata()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        List<ArtistEntity> artists = _artistEntityFixture.CreateMany(3);
        foreach (ArtistEntity artist in artists)
            artist.LibraryId = libraryId;
        _mockContext.Artists.AddRange(artists);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistLiteRow>> result = await _sut.GetAllLiteAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        PaginatedResultDto<ArtistLiteRow> page = result.Value;
        Assert.Equal(3, page.Count);
        Assert.Equal(3, page.Data.Count);
        Assert.Equal(1, page.CurrentPage);
        Assert.Equal(3, page.PerPage);
        Assert.Equal(1, page.NumberOfPages);
    }

    [Theory]
    [InlineData(1, 2, 1, 2)] // The first page is fully returned.
    [InlineData(2, 2, 2, 2)] // A middle page is fully returned.
    [InlineData(3, 2, 3, 1)] // The last page is partially returned.
    [InlineData(99, 2, 3, 1)] // A page beyond the last one is clamped to the last page.
    public async Task GetAllLiteAsync_WhenPaginationIsProvided_ShouldReturnTheRequestedPage(int requestedPage, int perPage, int expectedCurrentPage, int expectedRowCount)
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        List<ArtistEntity> artists = _artistEntityFixture.CreateMany(5);
        foreach (ArtistEntity artist in artists)
            artist.LibraryId = libraryId;
        _mockContext.Artists.AddRange(artists);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: requestedPage, perPage: perPage);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<ArtistLiteRow>> result = await _sut.GetAllLiteAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        PaginatedResultDto<ArtistLiteRow> page = result.Value;
        Assert.Equal(5, page.Count);
        Assert.Equal(3, page.NumberOfPages);
        Assert.Equal(expectedCurrentPage, page.CurrentPage);
        Assert.Equal(perPage, page.PerPage);
        Assert.Equal(expectedRowCount, page.Data.Count);
    }

    [Fact]
    public async Task GetArtistsNeedingMetadataAsync_WhenAnArtistAndAllItsDescendantsAreEnriched_ShouldNotReturnIt()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity enriched = CreateArtistWithFullAggregate();
        enriched.LibraryId = libraryId;
        MarkEnriched(enriched);
        ArtistEntity pending = _artistEntityFixture.Create(libraryId: libraryId, name: "Pending Artist", includeAlbums: false, includeContributors: false);
        _mockContext.Artists.AddRange(enriched, pending);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<ArtistEntity>> result = await _sut.GetArtistsNeedingMetadataAsync(libraryId, null, 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ArtistEntity onlyArtist = Assert.Single(result.Value);
        Assert.Equal(pending.Id, onlyArtist.Id);
    }

    [Fact]
    public async Task GetArtistsNeedingMetadataAsync_WhenATrackIsNotEnriched_ShouldReturnTheArtist()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = CreateArtistWithFullAggregate();
        artist.LibraryId = libraryId;
        MarkEnriched(artist);
        artist.Albums[0].Tracks[0].MetadataStatus = MetadataStatus.Pending;
        _mockContext.Artists.Add(artist);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<ArtistEntity>> result = await _sut.GetArtistsNeedingMetadataAsync(libraryId, null, 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ArtistEntity onlyArtist = Assert.Single(result.Value);
        Assert.Equal(artist.Id, onlyArtist.Id);
    }

    [Fact]
    public async Task GetArtistsNeedingMetadataAsync_WhenLastNameIsProvided_ShouldReturnOnlyArtistsAfterIt()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity alpha = _artistEntityFixture.Create(libraryId: libraryId, name: "Alpha", includeAlbums: false, includeContributors: false);
        ArtistEntity beta = _artistEntityFixture.Create(libraryId: libraryId, name: "Beta", includeAlbums: false, includeContributors: false);
        ArtistEntity gamma = _artistEntityFixture.Create(libraryId: libraryId, name: "Gamma", includeAlbums: false, includeContributors: false);
        _mockContext.Artists.AddRange(alpha, beta, gamma);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<ArtistEntity>> result = await _sut.GetArtistsNeedingMetadataAsync(libraryId, "Beta", 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ArtistEntity onlyArtist = Assert.Single(result.Value);
        Assert.Equal(gamma.Id, onlyArtist.Id);
    }

    [Fact]
    public async Task GetArtistsNeedingMetadataCountAsync_ShouldCountArtistsWithPendingMetadataIncludingDescendants()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity enriched = CreateArtistWithFullAggregate();
        enriched.LibraryId = libraryId;
        MarkEnriched(enriched);
        ArtistEntity pending = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        ArtistEntity otherLibrary = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        _mockContext.Artists.AddRange(enriched, pending, otherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<int> result = await _sut.GetArtistsNeedingMetadataCountAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(1, result.Value);
    }

    /// <summary>
    /// Marks the provided artist, its albums and its tracks as enriched.
    /// </summary>
    /// <param name="artist">The artist whose metadata enrichment status is set.</param>
    private static void MarkEnriched(ArtistEntity artist)
    {
        artist.MetadataStatus = MetadataStatus.Enriched;
        foreach (AlbumEntity album in artist.Albums)
        {
            album.MetadataStatus = MetadataStatus.Enriched;
            foreach (TrackEntity track in album.Tracks)
                track.MetadataStatus = MetadataStatus.Enriched;
        }
    }

    /// <summary>
    /// Creates an artist that owns an album which in turn owns a track, with every related collection of the aggregate populated deterministically.
    /// </summary>
    /// <returns>The created artist.</returns>
    private ArtistEntity CreateArtistWithFullAggregate()
    {
        ArtistEntity artist = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        artist.Contributors =
            [_artistContributorEntityFixture.Create(artistId: artist.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)];
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: artist.LibraryId, includeTracks: false, includeMetadata: false);
        album.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100)];
        album.Tags = [_tagEntityFixture.Create(name: "AlbumTag1"), _tagEntityFixture.Create(name: "AlbumTag2")];
        album.Genres = [_genreEntityFixture.Create(name: "AlbumGenre1"), _genreEntityFixture.Create(name: "AlbumGenre2")];
        album.Contributors = [_albumContributorEntityFixture.Create(albumId: album.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Producer)];
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: artist.LibraryId, includeMetadata: false);
        track.Ratings = [_audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.User, voteCount: 10)];
        track.Tags = [_tagEntityFixture.Create(name: "TrackTag1"), _tagEntityFixture.Create(name: "TrackTag2")];
        track.Genres = [_genreEntityFixture.Create(name: "TrackGenre1"), _genreEntityFixture.Create(name: "TrackGenre2")];
        track.Moods = [_trackMoodEntityFixture.Create(name: "Calm"), _trackMoodEntityFixture.Create(name: "Upbeat")];
        track.Isrcs = [_trackIsrcEntityFixture.Create(value: "USRC12345678"), _trackIsrcEntityFixture.Create(value: "GBAYE0000001")];
        track.Contributors = [_trackContributorEntityFixture.Create(trackId: track.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)];
        album.Tracks = [track];
        artist.Albums = [album];
        return artist;
    }
}
