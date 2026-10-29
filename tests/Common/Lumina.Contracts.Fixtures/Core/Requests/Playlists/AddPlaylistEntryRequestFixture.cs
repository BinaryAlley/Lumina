#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Playlists;

/// <summary>
/// Fixture class for the <see cref="AddPlaylistEntryRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntryRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="AddPlaylistEntryRequest"/>.
    /// </summary>
    /// <param name="trackId">Optional. The Id of the track to add.</param>
    /// <param name="includeTrackId">Whether the track Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AddPlaylistEntryRequest"/>.</returns>
    public AddPlaylistEntryRequest Create(
        Guid? trackId = null,
        bool includeTrackId = true)
    {
        return new AddPlaylistEntryRequest(
            includeTrackId ? (trackId ?? Guid.NewGuid()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="AddPlaylistEntryRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AddPlaylistEntryRequest"/> instances.</returns>
    public List<AddPlaylistEntryRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
