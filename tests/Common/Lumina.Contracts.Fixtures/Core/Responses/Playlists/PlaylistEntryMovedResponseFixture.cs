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
/// Fixture class for the <see cref="PlaylistEntryMovedResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryMovedResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaylistEntryMovedResponse"/>.
    /// </summary>
    /// <param name="playlistId">Optional. The Id of the playlist whose entry was moved.</param>
    /// <param name="entryId">Optional. The Id of the moved entry.</param>
    /// <param name="position">Optional. The new zero based position of the entry.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the playlist was last updated.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update time should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="PlaylistEntryMovedResponse"/>.</returns>
    public PlaylistEntryMovedResponse Create(
        Guid? playlistId = null,
        Guid? entryId = null,
        int? position = null,
        DateTime? updatedOnUtc = null,
        bool includeUpdatedOnUtc = true)
    {
        return new PlaylistEntryMovedResponse(
            playlistId ?? _faker.Random.Guid(),
            entryId ?? _faker.Random.Guid(),
            position ?? _faker.Random.Int(0, 1_000),
            includeUpdatedOnUtc ? (updatedOnUtc ?? DateTime.UtcNow.AddMinutes(-_faker.Random.Int(1, 1_440))) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="PlaylistEntryMovedResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistEntryMovedResponse"/> instances.</returns>
    public List<PlaylistEntryMovedResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
