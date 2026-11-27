#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetDeletedArtists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetDeletedArtists;

/// <summary>
/// Fixture class for the <see cref="GetDeletedArtistsQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedArtistsQueryFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetDeletedArtistsQuery"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose deleted artists are retrieved, taken from the route.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetDeletedArtistsQuery"/>.</returns>
    public GetDeletedArtistsQuery Create(
        string? libraryId = null,
        bool includeLibraryId = true)
    {
        return new GetDeletedArtistsQuery(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetDeletedArtistsQuery"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetDeletedArtistsQuery"/> instances.</returns>
    public List<GetDeletedArtistsQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
