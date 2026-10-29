#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Playlists;

/// <summary>
/// Fixture class for the <see cref="AddPlaylistEntryRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntryRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="AddPlaylistEntryRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="trackId">Optional Id of the track to add.</param>
    /// <param name="includeTrackId">Whether the track Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="AddPlaylistEntryRequest"/> instance.</returns>
    public AddPlaylistEntryRequest Create(
        Guid? trackId = null,
        bool includeTrackId = true)
    {
        return new AddPlaylistEntryRequest(
            TrackId: includeTrackId ? (trackId ?? Guid.NewGuid()) : null);
    }

    /// <summary>
    /// Creates multiple <see cref="AddPlaylistEntryRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AddPlaylistEntryRequest"/> instances.</returns>
    public List<AddPlaylistEntryRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
