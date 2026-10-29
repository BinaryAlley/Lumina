#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistEntriesAddedResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntriesAddedResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaylistEntriesAddedResponse"/>.
    /// </summary>
    /// <param name="playlistId">Optional. The Id of the playlist the entries were added to.</param>
    /// <param name="addedCount">Optional. The number of entries that were appended.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the playlist was last updated.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update time should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="PlaylistEntriesAddedResponse"/>.</returns>
    public PlaylistEntriesAddedResponse Create(
        Guid? playlistId = null,
        int? addedCount = null,
        DateTime? updatedOnUtc = null,
        bool includeUpdatedOnUtc = true)
    {
        return new PlaylistEntriesAddedResponse(
            playlistId ?? _faker.Random.Guid(),
            addedCount ?? _faker.Random.Int(0, 1_000),
            includeUpdatedOnUtc ? (updatedOnUtc ?? DateTime.UtcNow.AddMinutes(-_faker.Random.Int(1, 1_440))) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="PlaylistEntriesAddedResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistEntriesAddedResponse"/> instances.</returns>
    public List<PlaylistEntriesAddedResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
