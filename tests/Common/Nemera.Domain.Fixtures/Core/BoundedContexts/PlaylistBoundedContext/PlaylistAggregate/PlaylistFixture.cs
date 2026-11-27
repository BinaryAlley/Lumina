#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;

/// <summary>
/// Fixture class for the <see cref="Playlist"/> domain aggregate.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistFixture
{
    private readonly Faker _faker = new();
    private readonly PlaylistIdFixture _playlistIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="Playlist"/> domain aggregate, with pre-existing data.
    /// </summary>
    /// <param name="id">Optional. The Id of the playlist.</param>
    /// <param name="userId">Optional. The Id of the user that owns the playlist.</param>
    /// <param name="name">Optional. The name of the playlist.</param>
    /// <param name="description">Optional. The description of the playlist.</param>
    /// <param name="type">Optional. The kind of media the playlist holds. Defaults to the only supported kind.</param>
    /// <param name="entries">Optional. The ordered entries of the playlist. Defaults to no entries.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the playlist was created. Defaults to a few days in the past.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the playlist was last updated.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to no value.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update date and time should be included, or forced to no value.</param>
    /// <returns>The created <see cref="Playlist"/>.</returns>
    /// <remarks>
    /// The playlist is built through the rehydrating <see cref="Playlist.Create(PlaylistId, UserId, string, Optional{string}, PlaylistType, List{PlaylistEntry}, DateTime, Optional{DateTime})"/>
    /// overload, so it carries no domain events and is never marked as deleted.
    /// </remarks>
    public Playlist Create(
        PlaylistId? id = null,
        UserId? userId = null,
        string? name = null,
        string? description = null,
        PlaylistType? type = null,
        IEnumerable<PlaylistEntry>? entries = null,
        DateTime? createdOnUtc = null,
        DateTime? updatedOnUtc = null,
        bool includeDescription = true,
        bool includeUpdatedOnUtc = false)
    {
        return Playlist.Create(
            id ?? _playlistIdFixture.Create(),
            userId ?? _userIdFixture.Create(),
            name ?? _faker.Lorem.Sentence(3),
            includeDescription ? Optional<string>.FromNullable(description ?? _faker.Lorem.Sentence()) : Optional<string>.None(),
            type ?? PlaylistType.Music,
            entries is null ? [] : [.. entries],
            createdOnUtc ?? DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 30)).AddMinutes(-_faker.Random.Int(0, 59)),
            includeUpdatedOnUtc ? Optional<DateTime>.Some(updatedOnUtc ?? DateTime.UtcNow) : Optional<DateTime>.None()).Value;
    }

    /// <summary>
    /// Creates multiple <see cref="Playlist"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Playlist"/> instances.</returns>
    public List<Playlist> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
