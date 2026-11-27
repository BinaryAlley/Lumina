#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.DataAccess.Core.Repositories.MusicLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.MusicLibrary;

/// <summary>
/// Contains unit tests for the <see cref="MusicArtworkRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtworkRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly MusicArtworkRepository _sut;
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicArtworkRepositoryTests"/> class.
    /// </summary>
    public MusicArtworkRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new MusicArtworkRepository(_mockContext);
    }

    [Fact]
    public async Task GetByOwnersAsync_WhenCalled_ShouldReturnOnlyTheArtworkOfTheRequestedOwnerTypeAndOwnerIds()
    {
        // Arrange
        Guid firstOwnerId = Guid.NewGuid();
        Guid secondOwnerId = Guid.NewGuid();
        MusicArtworkEntity firstOwnerCover = _musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album, ownerId: firstOwnerId, artworkType: ArtworkType.Cover, ordinal: 0);
        MusicArtworkEntity firstOwnerFront = _musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album, ownerId: firstOwnerId, artworkType: ArtworkType.Front, ordinal: 1);
        MusicArtworkEntity secondOwnerCover = _musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album, ownerId: secondOwnerId, artworkType: ArtworkType.Cover, ordinal: 0);
        MusicArtworkEntity artworkOfAnotherOwner = _musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album, ownerId: Guid.NewGuid(), artworkType: ArtworkType.Cover, ordinal: 0);
        MusicArtworkEntity artworkOfAnotherOwnerType = _musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Artist, ownerId: firstOwnerId, artworkType: ArtworkType.Cover, ordinal: 0);
        _mockContext.MusicArtwork.AddRange(
            firstOwnerCover, firstOwnerFront, secondOwnerCover, artworkOfAnotherOwner, artworkOfAnotherOwnerType);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MusicArtworkEntity>> result = await _sut.GetByOwnersAsync(MusicArtworkOwnerType.Album, [firstOwnerId, secondOwnerId], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count);
        Assert.Contains(result.Value, artwork => artwork.Id == firstOwnerCover.Id);
        Assert.Contains(result.Value, artwork => artwork.Id == firstOwnerFront.Id);
        Assert.Contains(result.Value, artwork => artwork.Id == secondOwnerCover.Id);
        Assert.DoesNotContain(result.Value, artwork => artwork.Id == artworkOfAnotherOwner.Id);
        Assert.DoesNotContain(result.Value, artwork => artwork.Id == artworkOfAnotherOwnerType.Id);
    }

    [Fact]
    public async Task GetByOwnersAsync_WhenAnOwnerIdDoesNotExist_ShouldIgnoreIt()
    {
        // Arrange
        Guid ownerId = Guid.NewGuid();
        MusicArtworkEntity artwork = _musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Track, ownerId: ownerId, artworkType: ArtworkType.Cover, ordinal: 0);
        _mockContext.MusicArtwork.Add(artwork);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MusicArtworkEntity>> result = await _sut.GetByOwnersAsync(MusicArtworkOwnerType.Track, [ownerId, Guid.NewGuid()], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        MusicArtworkEntity returnedArtwork = Assert.Single(result.Value);
        Assert.Equal(artwork.Id, returnedArtwork.Id);
    }

    [Fact]
    public async Task GetByOwnersAsync_WhenOwnerIdsIsEmpty_ShouldReturnAnEmptyList()
    {
        // Arrange
        MusicArtworkEntity artwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album);
        _mockContext.MusicArtwork.Add(artwork);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MusicArtworkEntity>> result = await _sut.GetByOwnersAsync(MusicArtworkOwnerType.Album, [], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetByOwnersAsync_WhenCalled_ShouldOrderTheArtworkByTypeThenOrdinal()
    {
        // Arrange
        Guid ownerId = Guid.NewGuid();
        MusicArtworkEntity frontSecond = _musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album, ownerId: ownerId, artworkType: ArtworkType.Front, ordinal: 1);
        MusicArtworkEntity frontFirst = _musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album, ownerId: ownerId, artworkType: ArtworkType.Front, ordinal: 0);
        MusicArtworkEntity cover = _musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album, ownerId: ownerId, artworkType: ArtworkType.Cover, ordinal: 0);
        _mockContext.MusicArtwork.AddRange(frontSecond, frontFirst, cover);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MusicArtworkEntity>> result = await _sut.GetByOwnersAsync(MusicArtworkOwnerType.Album, [ownerId], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal([cover.Id, frontFirst.Id, frontSecond.Id], result.Value.Select(artwork => artwork.Id));
    }

    [Fact]
    public async Task GetByOwnersAsync_WhenMoreThanFiveHundredOwnerIdsAreRequested_ShouldReturnTheArtworkOfEveryOwner()
    {
        // Arrange
        const int OWNER_COUNT = 501;
        List<Guid> ownerIds = [.. Enumerable.Range(0, OWNER_COUNT).Select(_ => Guid.NewGuid())];
        List<MusicArtworkEntity> artwork = [.. ownerIds.Select(ownerId => _musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album, ownerId: ownerId, artworkType: ArtworkType.Cover, ordinal: 0))];
        _mockContext.MusicArtwork.AddRange(artwork);
        await _mockContext.SaveChangesAsync();

        // Act
        // The lookup is chunked in batches of 500, so the 501st owner proves the last chunk is queried as well.
        Result<IReadOnlyList<MusicArtworkEntity>> result = await _sut.GetByOwnersAsync(MusicArtworkOwnerType.Album, ownerIds, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(OWNER_COUNT, result.Value.Count);
        Assert.Equal(
            artwork.Select(candidate => candidate.Id).OrderBy(id => id),
            result.Value.Select(candidate => candidate.Id).OrderBy(id => id));
    }
}
