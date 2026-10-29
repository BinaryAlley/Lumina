#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.Events;

/// <summary>
/// Fixture class for the <see cref="FavoriteRemovedDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteRemovedDomainEventFixture
{
    private readonly Faker _faker = new();
    private readonly FavoriteIdFixture _favoriteIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();
    private readonly FavoriteMediaKind[] _supportedMediaKinds =
    [
        FavoriteMediaKind.MusicArtist,
        FavoriteMediaKind.MusicAlbum,
        FavoriteMediaKind.MusicTrack,
        FavoriteMediaKind.Book
    ];

    /// <summary>
    /// Creates a random valid <see cref="FavoriteRemovedDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="favoriteId">Optional. The unique identifier of the removed favorite.</param>
    /// <param name="userId">Optional. The unique identifier of the user that owned the favorite.</param>
    /// <param name="mediaItemId">Optional. The unique identifier of the media item that was unfavorited.</param>
    /// <param name="mediaKind">Optional. The kind of the media item that was unfavorited. Defaults to a random supported kind.</param>
    /// <param name="occurredOnUtc">Optional. The date and time when the domain event occurred.</param>
    /// <returns>The created <see cref="FavoriteRemovedDomainEvent"/>.</returns>
    public FavoriteRemovedDomainEvent Create(
        Guid? id = null,
        FavoriteId? favoriteId = null,
        UserId? userId = null,
        MediaItemId? mediaItemId = null,
        FavoriteMediaKind? mediaKind = null,
        DateTime? occurredOnUtc = null)
    {
        return new FavoriteRemovedDomainEvent(
            id ?? Guid.NewGuid(),
            favoriteId ?? _favoriteIdFixture.Create(),
            userId ?? _userIdFixture.Create(),
            mediaItemId ?? _mediaItemIdFixture.Create(),
            mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds),
            occurredOnUtc ?? DateTime.UtcNow);
    }

    /// <summary>
    /// Creates multiple <see cref="FavoriteRemovedDomainEvent"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FavoriteRemovedDomainEvent"/> instances.</returns>
    public List<FavoriteRemovedDomainEvent> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
