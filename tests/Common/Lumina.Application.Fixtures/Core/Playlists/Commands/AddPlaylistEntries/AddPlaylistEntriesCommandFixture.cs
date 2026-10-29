#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.AddPlaylistEntries;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Playlists.Commands.AddPlaylistEntries;

/// <summary>
/// Fixture class for the <see cref="AddPlaylistEntriesCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntriesCommandFixture
{
    /// <summary>
    /// Creates a random valid <see cref="AddPlaylistEntriesCommand"/>.
    /// </summary>
    /// <param name="playlistId">Optional. The Id of the playlist, taken from the route.</param>
    /// <param name="trackIds">Optional. The Ids of the tracks to add, in order.</param>
    /// <param name="trackCount">Optional. The number of random track Ids to generate when <paramref name="trackIds"/> is not supplied.</param>
    /// <param name="includePlaylistId">Whether the playlist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTrackIds">Whether the track Ids should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AddPlaylistEntriesCommand"/>.</returns>
    public AddPlaylistEntriesCommand Create(
        string? playlistId = null,
        List<Guid>? trackIds = null,
        int trackCount = 3,
        bool includePlaylistId = true,
        bool includeTrackIds = true)
    {
        return new AddPlaylistEntriesCommand(
            includePlaylistId ? (playlistId ?? Guid.NewGuid().ToString()) : null,
            includeTrackIds ? (trackIds ?? CreateRandomTrackIds(trackCount)) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="AddPlaylistEntriesCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AddPlaylistEntriesCommand"/> instances.</returns>
    public List<AddPlaylistEntriesCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Creates the requested number of distinct random track Ids.
    /// </summary>
    /// <param name="count">The number of track Ids to create.</param>
    /// <returns>The created track Ids.</returns>
    private static List<Guid> CreateRandomTrackIds(int count)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Guid.NewGuid())];
    }
}
