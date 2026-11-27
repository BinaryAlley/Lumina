#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetDeletedTracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetDeletedTracks;

/// <summary>
/// Fixture class for the <see cref="GetDeletedTracksQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedTracksQueryFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetDeletedTracksQuery"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose deleted tracks are retrieved, taken from the route.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetDeletedTracksQuery"/>.</returns>
    public GetDeletedTracksQuery Create(
        string? libraryId = null,
        bool includeLibraryId = true)
    {
        return new GetDeletedTracksQuery(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetDeletedTracksQuery"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetDeletedTracksQuery"/> instances.</returns>
    public List<GetDeletedTracksQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
