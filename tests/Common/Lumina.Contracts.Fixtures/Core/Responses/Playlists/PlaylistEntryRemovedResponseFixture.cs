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
/// Fixture class for the <see cref="PlaylistEntryRemovedResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryRemovedResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaylistEntryRemovedResponse"/>.
    /// </summary>
    /// <param name="playlistId">Optional. The Id of the playlist the entry was removed from.</param>
    /// <param name="removedEntryId">Optional. The Id of the removed entry.</param>
    /// <param name="repositionedCount">Optional. The number of entries that were repositioned, which the contract always reports as zero.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the playlist was last updated.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update time should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="PlaylistEntryRemovedResponse"/>.</returns>
    public PlaylistEntryRemovedResponse Create(
        Guid? playlistId = null,
        Guid? removedEntryId = null,
        int? repositionedCount = null,
        DateTime? updatedOnUtc = null,
        bool includeUpdatedOnUtc = true)
    {
        return new PlaylistEntryRemovedResponse(
            playlistId ?? _faker.Random.Guid(),
            removedEntryId ?? _faker.Random.Guid(),
            repositionedCount ?? 0,
            includeUpdatedOnUtc ? (updatedOnUtc ?? DateTime.UtcNow.AddMinutes(-_faker.Random.Int(1, 1_440))) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="PlaylistEntryRemovedResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistEntryRemovedResponse"/> instances.</returns>
    public List<PlaylistEntryRemovedResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
