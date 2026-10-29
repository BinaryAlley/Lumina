#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Playlists;

/// <summary>
/// Fixture class for the <see cref="AddPlaylistEntriesRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntriesRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="AddPlaylistEntriesRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="trackIds">Optional Ids of the tracks to add, in order.</param>
    /// <param name="trackCount">Optional number of random track Ids to generate when <paramref name="trackIds"/> is not supplied.</param>
    /// <param name="includeTrackIds">Whether the track Ids should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="AddPlaylistEntriesRequest"/> instance.</returns>
    public AddPlaylistEntriesRequest Create(
        List<Guid>? trackIds = null,
        int trackCount = 3,
        bool includeTrackIds = true)
    {
        return new AddPlaylistEntriesRequest(
            TrackIds: includeTrackIds ? (trackIds ?? CreateRandomTrackIds(trackCount)) : null);
    }

    /// <summary>
    /// Creates multiple <see cref="AddPlaylistEntriesRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AddPlaylistEntriesRequest"/> instances.</returns>
    public List<AddPlaylistEntriesRequest> CreateMany(int count = 3)
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
