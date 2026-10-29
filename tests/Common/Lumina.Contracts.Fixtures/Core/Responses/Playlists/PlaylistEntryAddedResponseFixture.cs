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
/// Fixture class for the <see cref="PlaylistEntryAddedResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryAddedResponseFixture
{
    private readonly Faker _faker = new();
    private readonly PlaylistEntryResponseFixture _playlistEntryResponseFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaylistEntryAddedResponse"/>.
    /// </summary>
    /// <param name="playlistId">Optional. The Id of the playlist the entry was added to.</param>
    /// <param name="entry">Optional. The added entry.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the playlist was last updated.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update time should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="PlaylistEntryAddedResponse"/>.</returns>
    public PlaylistEntryAddedResponse Create(
        Guid? playlistId = null,
        PlaylistEntryResponse? entry = null,
        DateTime? updatedOnUtc = null,
        bool includeUpdatedOnUtc = true)
    {
        return new PlaylistEntryAddedResponse(
            playlistId ?? _faker.Random.Guid(),
            entry ?? _playlistEntryResponseFixture.Create(),
            includeUpdatedOnUtc ? (updatedOnUtc ?? DateTime.UtcNow.AddMinutes(-_faker.Random.Int(1, 1_440))) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="PlaylistEntryAddedResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistEntryAddedResponse"/> instances.</returns>
    public List<PlaylistEntryAddedResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
