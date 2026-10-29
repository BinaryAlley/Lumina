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
/// Fixture class for the <see cref="FavoriteAddedDomainEvent"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteAddedDomainEventFixture
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
    /// Creates a random valid <see cref="FavoriteAddedDomainEvent"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the domain event.</param>
    /// <param name="favoriteId">Optional. The unique identifier of the favorite.</param>
    /// <param name="userId">Optional. The unique identifier of the user that favorited the media item.</param>
    /// <param name="mediaItemId">Optional. The unique identifier of the favorited media item.</param>
    /// <param name="mediaKind">Optional. The kind of the favorited media item. Defaults to a random supported kind.</param>
    /// <param name="occurredOnUtc">Optional. The date and time when the domain event occurred.</param>
    /// <returns>The created <see cref="FavoriteAddedDomainEvent"/>.</returns>
    public FavoriteAddedDomainEvent Create(
        Guid? id = null,
        FavoriteId? favoriteId = null,
        UserId? userId = null,
        MediaItemId? mediaItemId = null,
        FavoriteMediaKind? mediaKind = null,
        DateTime? occurredOnUtc = null)
    {
        return new FavoriteAddedDomainEvent(
            id ?? Guid.NewGuid(),
            favoriteId ?? _favoriteIdFixture.Create(),
            userId ?? _userIdFixture.Create(),
            mediaItemId ?? _mediaItemIdFixture.Create(),
            mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds),
            occurredOnUtc ?? DateTime.UtcNow);
    }

    /// <summary>
    /// Creates multiple <see cref="FavoriteAddedDomainEvent"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FavoriteAddedDomainEvent"/> instances.</returns>
    public List<FavoriteAddedDomainEvent> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
