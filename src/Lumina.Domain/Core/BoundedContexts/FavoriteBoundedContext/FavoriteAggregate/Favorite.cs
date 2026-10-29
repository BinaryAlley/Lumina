#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;

/// <summary>
/// Aggregate root for a favorite, the marker a user places on a media item.
/// </summary>
[DebuggerDisplay("Id: {Id.Value}; MediaKind: {MediaKind}; MediaItemId: {MediaItemId.Value}")]
public sealed class Favorite : AggregateRoot<FavoriteId>
{
    /// <summary>
    /// Gets the object representing the unique identifier of the user that owns the favorite.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Gets the Id of the favorited media item.
    /// </summary>
    public MediaItemId MediaItemId { get; private set; }

    /// <summary>
    /// Gets the kind of the favorited media item.
    /// </summary>
    public FavoriteMediaKind MediaKind { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the favorite was marked for removal in this unit of work.
    /// </summary>
    public bool IsRemoved { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Favorite"/> class.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the favorite.</param>
    /// <param name="userId">The object representing the unique identifier of the user that owns the favorite.</param>
    /// <param name="mediaItemId">The Id of the favorited media item.</param>
    /// <param name="mediaKind">The kind of the favorited media item.</param>
    /// <param name="createdOnUtc">The date and time when the favorite was created.</param>
    /// <param name="updatedOnUtc">The date and time when the favorite was last updated.</param>
    private Favorite(
        FavoriteId id,
        UserId userId,
        MediaItemId mediaItemId,
        FavoriteMediaKind mediaKind,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc) : base(id)
    {
        Id = id;
        UserId = userId;
        MediaItemId = mediaItemId;
        MediaKind = mediaKind;
        CreatedOnUtc = createdOnUtc;
        UpdatedOnUtc = updatedOnUtc;
    }

    /// <summary>
    /// Creates a new favorite of the media item identified by <paramref name="mediaItemId"/> for the user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The object representing the unique identifier of the user that owns the favorite.</param>
    /// <param name="mediaItemId">The Id of the media item to favorite.</param>
    /// <param name="mediaKind">The kind of the media item to favorite.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Favorite"/>, or an error message.
    /// </returns>
    public static Result<Favorite> Create(UserId userId, MediaItemId mediaItemId, FavoriteMediaKind mediaKind)
    {
        // A planned media kind is part of the model so the storage and the contracts are ready for it, but its items cannot be favorited until its library exists.
        if (!IsSupported(mediaKind))
            return Errors.Favorites.FavoriteMediaKindIsNotSupported;

        DateTime occurredOnUtc = DateTime.UtcNow;
        Favorite favorite = new(
            FavoriteId.CreateUnique(),
            userId,
            mediaItemId,
            mediaKind,
            occurredOnUtc,
            Optional<DateTime>.None());

        favorite._domainEvents.Add(new FavoriteAddedDomainEvent(
            Guid.NewGuid(),
            favorite.Id,
            userId,
            mediaItemId,
            mediaKind,
            occurredOnUtc));
        return favorite;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Favorite"/> class, with pre-existing data.
    /// </summary>
    /// <param name="id">The object representing the unique identifier of the favorite.</param>
    /// <param name="userId">The object representing the unique identifier of the user that owns the favorite.</param>
    /// <param name="mediaItemId">The Id of the favorited media item.</param>
    /// <param name="mediaKind">The kind of the favorited media item.</param>
    /// <param name="createdOnUtc">The date and time when the favorite was created.</param>
    /// <param name="updatedOnUtc">The date and time when the favorite was last updated.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="Favorite"/>, or an error message.
    /// </returns>
    public static Result<Favorite> Create(
        FavoriteId id,
        UserId userId,
        MediaItemId mediaItemId,
        FavoriteMediaKind mediaKind,
        DateTime createdOnUtc,
        Optional<DateTime> updatedOnUtc)
    {
        // A stored favorite is rehydrated as it was stored, whether or not its media kind is supported today.
        return new Favorite(id, userId, mediaItemId, mediaKind, createdOnUtc, updatedOnUtc);
    }

    /// <summary>
    /// Marks the favorite as removed, raising the domain event that announces its removal.
    /// </summary>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Deleted> Remove()
    {
        if (IsRemoved)
            return Result.Deleted;

        // Earlier events become irrelevant once the favorite is removed.
        _domainEvents.Clear();
        IsRemoved = true;
        _domainEvents.Add(new FavoriteRemovedDomainEvent(
            Guid.NewGuid(),
            Id,
            UserId,
            MediaItemId,
            MediaKind,
            DateTime.UtcNow));
        return Result.Deleted;
    }

    /// <summary>
    /// Determines whether media items of <paramref name="mediaKind"/> can be favorited today.
    /// </summary>
    /// <param name="mediaKind">The kind of media item.</param>
    /// <returns><see langword="true"/> if items of the media kind can be favorited, <see langword="false"/> otherwise.</returns>
    public static bool IsSupported(FavoriteMediaKind mediaKind)
    {
        return mediaKind switch
        {
            FavoriteMediaKind.MusicArtist => true,
            FavoriteMediaKind.MusicAlbum => true,
            FavoriteMediaKind.MusicTrack => true,
            FavoriteMediaKind.Book => true,
            _ => false
        };
    }
}
