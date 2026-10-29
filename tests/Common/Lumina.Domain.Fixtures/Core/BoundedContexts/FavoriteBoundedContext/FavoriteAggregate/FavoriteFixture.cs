#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;
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

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;

/// <summary>
/// Fixture class for the <see cref="Favorite"/> domain aggregate.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteFixture
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
    /// Creates a random valid <see cref="Favorite"/> domain aggregate, with pre-existing data.
    /// </summary>
    /// <param name="id">Optional. The Id of the favorite.</param>
    /// <param name="userId">Optional. The Id of the user that owns the favorite.</param>
    /// <param name="mediaItemId">Optional. The Id of the favorited media item.</param>
    /// <param name="mediaKind">Optional. The kind of the favorited media item. Defaults to a random supported kind.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the favorite was created. Defaults to a few days in the past.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the favorite was last updated. Defaults to a moment at or after the creation date, or to no value when <paramref name="includeUpdatedOnUtc"/> is <see langword="false"/>.</param>
    /// <param name="includeUpdatedOnUtc">Whether to generate an update date and time when <paramref name="updatedOnUtc"/> is not supplied.</param>
    /// <returns>The created <see cref="Favorite"/>.</returns>
    /// <remarks>
    /// The favorite is built through the rehydrating <see cref="Favorite.Create(FavoriteId, UserId, MediaItemId, FavoriteMediaKind, DateTime, Optional{DateTime})"/>
    /// overload, so it carries no domain events, is never marked as removed, and keeps whatever media kind it is given, including a planned one.
    /// </remarks>
    public Favorite Create(
        FavoriteId? id = null,
        UserId? userId = null,
        MediaItemId? mediaItemId = null,
        FavoriteMediaKind? mediaKind = null,
        DateTime? createdOnUtc = null,
        Optional<DateTime>? updatedOnUtc = null,
        bool includeUpdatedOnUtc = true)
    {
        DateTime resolvedCreatedOnUtc = createdOnUtc ?? DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 30)).AddMinutes(-_faker.Random.Int(0, 59));
        return Favorite.Create(
            id ?? _favoriteIdFixture.Create(),
            userId ?? _userIdFixture.Create(),
            mediaItemId ?? _mediaItemIdFixture.Create(),
            mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds),
            resolvedCreatedOnUtc,
            ResolveUpdatedOnUtc(updatedOnUtc, includeUpdatedOnUtc, resolvedCreatedOnUtc)).Value;
    }

    /// <summary>
    /// Creates multiple <see cref="Favorite"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Favorite"/> instances.</returns>
    public List<Favorite> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Resolves the update date and time of the favorite, keeping it at or after the creation date and inside the date range.
    /// </summary>
    /// <param name="updatedOnUtc">The explicitly supplied update date and time, if any.</param>
    /// <param name="includeUpdatedOnUtc">Whether to generate an update date and time when none is supplied.</param>
    /// <param name="createdOnUtc">The creation date and time of the favorite.</param>
    /// <returns>The resolved update date and time, or no value when the favorite was never updated.</returns>
    private Optional<DateTime> ResolveUpdatedOnUtc(Optional<DateTime>? updatedOnUtc, bool includeUpdatedOnUtc, DateTime createdOnUtc)
    {
        if (updatedOnUtc.HasValue)
            return updatedOnUtc.Value;
        if (!includeUpdatedOnUtc)
            return Optional<DateTime>.None();
        TimeSpan offsetFromCreation = TimeSpan.FromMinutes(_faker.Random.Int(0, 1440));
        return Optional<DateTime>.Some(DateTime.MaxValue - createdOnUtc < offsetFromCreation
            ? DateTime.MaxValue
            : createdOnUtc.Add(offsetFromCreation));
    }
}
