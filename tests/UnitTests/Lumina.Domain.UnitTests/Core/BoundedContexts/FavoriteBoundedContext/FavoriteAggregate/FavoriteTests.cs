#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;

/// <summary>
/// Contains unit tests for the <see cref="Favorite"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteTests
{
    private readonly FavoriteFixture _favoriteFixture = new();
    private readonly FavoriteIdFixture _favoriteIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();

    [Theory]
    [InlineData(FavoriteMediaKind.MusicArtist)] // an artist of a music library
    [InlineData(FavoriteMediaKind.MusicAlbum)] // an album of a music library
    [InlineData(FavoriteMediaKind.MusicTrack)] // a track of a music library
    [InlineData(FavoriteMediaKind.Book)] // a book of a book library
    public void Create_WhenMediaKindIsSupported_ShouldCreateFavoriteAndRaiseSingleAddedEvent(FavoriteMediaKind mediaKind)
    {
        // Arrange
        UserId userId = _userIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        DateTime beforeUtc = DateTime.UtcNow;

        // Act
        Result<Favorite> result = Favorite.Create(userId, mediaItemId, mediaKind);

        // Assert
        Assert.False(result.IsFailure);
        Favorite favorite = result.Value;
        Assert.NotEqual(Guid.Empty, favorite.Id.Value);
        Assert.Equal(userId, favorite.UserId);
        Assert.Equal(mediaItemId, favorite.MediaItemId);
        Assert.Equal(mediaKind, favorite.MediaKind);
        Assert.InRange(favorite.CreatedOnUtc, beforeUtc, DateTime.UtcNow);
        Assert.False(favorite.UpdatedOnUtc.HasValue);
        Assert.False(favorite.IsRemoved);
        FavoriteAddedDomainEvent addedEvent = Assert.IsType<FavoriteAddedDomainEvent>(Assert.Single(favorite.GetDomainEvents()));
        Assert.NotEqual(Guid.Empty, addedEvent.Id);
        Assert.Equal(favorite.Id, addedEvent.FavoriteId);
        Assert.Equal(userId, addedEvent.UserId);
        Assert.Equal(mediaItemId, addedEvent.MediaItemId);
        Assert.Equal(mediaKind, addedEvent.MediaKind);
        Assert.Equal(favorite.CreatedOnUtc, addedEvent.OccurredOnUtc);
    }

    [Fact]
    public void Create_WhenCalledTwice_ShouldGenerateDistinctIds()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();

        // Act
        Result<Favorite> firstResult = Favorite.Create(userId, mediaItemId, FavoriteMediaKind.MusicTrack);
        Result<Favorite> secondResult = Favorite.Create(userId, mediaItemId, FavoriteMediaKind.MusicTrack);

        // Assert
        Assert.False(firstResult.IsFailure);
        Assert.False(secondResult.IsFailure);
        Assert.NotEqual(firstResult.Value.Id, secondResult.Value.Id);
    }

    [Theory]
    [InlineData(FavoriteMediaKind.None)] // the default value, which is not a kind that can be favorited
    [InlineData(FavoriteMediaKind.Video)] // a planned kind: video libraries are not catalogued yet
    [InlineData(FavoriteMediaKind.Photo)] // a planned kind: photo libraries are not catalogued yet
    [InlineData((FavoriteMediaKind)999)] // a value outside the declared enumeration members
    public void Create_WhenMediaKindIsNotSupported_ShouldReturnErrorAndProduceNoFavorite(FavoriteMediaKind mediaKind)
    {
        // Act
        Result<Favorite> result = Favorite.Create(_userIdFixture.Create(), _mediaItemIdFixture.Create(), mediaKind);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Favorites.FavoriteMediaKindIsNotSupported, result.FirstError);
        // A failed result carries no favorite, so no FavoriteAddedDomainEvent can have been raised for one.
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Theory]
    [InlineData(FavoriteMediaKind.MusicAlbum)] // a supported kind that was stored with an update
    [InlineData(FavoriteMediaKind.None)] // the default value, preserved as it was recorded
    [InlineData(FavoriteMediaKind.Video)] // a planned kind, stored as it was recorded, whatever its support today
    public void Create_WhenRehydratingWithUpdatedOnUtc_ShouldPreserveAllDataAndRaiseNoEvents(FavoriteMediaKind mediaKind)
    {
        // Arrange
        FavoriteId id = _favoriteIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        DateTime createdOnUtc = DateTime.UtcNow.AddDays(-10);
        DateTime updatedOnUtc = createdOnUtc.AddDays(2);

        // Act
        Result<Favorite> result = Favorite.Create(id, userId, mediaItemId, mediaKind, createdOnUtc, Optional<DateTime>.Some(updatedOnUtc));

        // Assert
        Assert.False(result.IsFailure);
        Favorite favorite = result.Value;
        Assert.Equal(id, favorite.Id);
        Assert.Equal(userId, favorite.UserId);
        Assert.Equal(mediaItemId, favorite.MediaItemId);
        Assert.Equal(mediaKind, favorite.MediaKind);
        Assert.Equal(createdOnUtc, favorite.CreatedOnUtc);
        Assert.Equal(Optional<DateTime>.Some(updatedOnUtc), favorite.UpdatedOnUtc);
        Assert.False(favorite.IsRemoved);
        Assert.Empty(favorite.GetDomainEvents());
    }

    [Fact]
    public void Create_WhenRehydratingWithoutUpdatedOnUtc_ShouldPreserveAllDataAndRaiseNoEvents()
    {
        // Arrange
        FavoriteId id = _favoriteIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        DateTime createdOnUtc = DateTime.UtcNow.AddDays(-10);

        // Act
        Result<Favorite> result = Favorite.Create(id, userId, mediaItemId, FavoriteMediaKind.Book, createdOnUtc, Optional<DateTime>.None());

        // Assert
        Assert.False(result.IsFailure);
        Favorite favorite = result.Value;
        Assert.Equal(id, favorite.Id);
        Assert.Equal(userId, favorite.UserId);
        Assert.Equal(mediaItemId, favorite.MediaItemId);
        Assert.Equal(FavoriteMediaKind.Book, favorite.MediaKind);
        Assert.Equal(createdOnUtc, favorite.CreatedOnUtc);
        Assert.False(favorite.UpdatedOnUtc.HasValue);
        Assert.False(favorite.IsRemoved);
        Assert.Empty(favorite.GetDomainEvents());
    }

    [Fact]
    public void Remove_WhenFavoriteIsNotRemoved_ShouldMarkItRemovedAndRaiseOnlyRemovedEvent()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        Favorite favorite = Favorite.Create(userId, mediaItemId, FavoriteMediaKind.MusicTrack).Value;
        FavoriteId favoriteId = favorite.Id;
        DateTime beforeUtc = DateTime.UtcNow;

        // Act
        Result<Deleted> result = favorite.Remove();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        Assert.True(favorite.IsRemoved);
        // The favorite was created with an added event, so a single remaining removal event also proves it was cleared.
        FavoriteRemovedDomainEvent removedEvent = Assert.IsType<FavoriteRemovedDomainEvent>(Assert.Single(favorite.GetDomainEvents()));
        Assert.NotEqual(Guid.Empty, removedEvent.Id);
        Assert.Equal(favoriteId, removedEvent.FavoriteId);
        Assert.Equal(userId, removedEvent.UserId);
        Assert.Equal(mediaItemId, removedEvent.MediaItemId);
        Assert.Equal(FavoriteMediaKind.MusicTrack, removedEvent.MediaKind);
        Assert.InRange(removedEvent.OccurredOnUtc, beforeUtc, DateTime.UtcNow);
    }

    [Fact]
    public void Remove_WhenCalledTwice_ShouldReturnDeletedWithoutRaisingAnotherEvent()
    {
        // Arrange
        Favorite favorite = Favorite.Create(_userIdFixture.Create(), _mediaItemIdFixture.Create(), FavoriteMediaKind.MusicAlbum).Value;
        Result<Deleted> firstResult = favorite.Remove();
        Assert.False(firstResult.IsFailure);
        Assert.Single(favorite.GetDomainEvents());

        // Act
        Result<Deleted> secondResult = favorite.Remove();

        // Assert
        Assert.False(secondResult.IsFailure);
        Assert.Equal(Result.Deleted, secondResult.Value);
        Assert.True(favorite.IsRemoved);
        Assert.Empty(favorite.GetDomainEvents());
    }

    [Fact]
    public void Remove_WhenRehydratedFavoriteIsAlreadyRemoved_ShouldReturnDeletedWithoutRaisingAnotherEvent()
    {
        // Arrange
        Favorite favorite = _favoriteFixture.Create(mediaKind: FavoriteMediaKind.Book);
        Result<Deleted> firstResult = favorite.Remove();
        Assert.False(firstResult.IsFailure);
        Assert.Single(favorite.GetDomainEvents());

        // Act
        Result<Deleted> secondResult = favorite.Remove();

        // Assert
        Assert.False(secondResult.IsFailure);
        Assert.Equal(Result.Deleted, secondResult.Value);
        Assert.True(favorite.IsRemoved);
        Assert.Empty(favorite.GetDomainEvents());
    }

    [Theory]
    [InlineData(FavoriteMediaKind.None, false)] // the default value, which is not a kind that can be favorited
    [InlineData(FavoriteMediaKind.MusicArtist, true)] // an artist of a music library can be favorited
    [InlineData(FavoriteMediaKind.MusicAlbum, true)] // an album of a music library can be favorited
    [InlineData(FavoriteMediaKind.MusicTrack, true)] // a track of a music library can be favorited
    [InlineData(FavoriteMediaKind.Book, true)] // a book of a book library can be favorited
    [InlineData(FavoriteMediaKind.Video, false)] // a planned kind: video libraries are not catalogued yet
    [InlineData(FavoriteMediaKind.Photo, false)] // a planned kind: photo libraries are not catalogued yet
    [InlineData((FavoriteMediaKind)999, false)] // a value outside the declared enumeration members
    [InlineData((FavoriteMediaKind)(-1), false)] // a value below the declared enumeration members
    public void IsSupported_WhenCalled_ShouldOnlySupportCataloguedMediaKinds(FavoriteMediaKind mediaKind, bool expectedIsSupported)
    {
        // Act
        bool isSupported = Favorite.IsSupported(mediaKind);

        // Assert
        Assert.Equal(expectedIsSupported, isSupported);
    }
}
