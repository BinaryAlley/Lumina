#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtist;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtist;

/// <summary>
/// Fixture class for the <see cref="GetArtistQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistQueryFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetArtistQuery"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the artist belongs to, taken from the route.</param>
    /// <param name="artistId">Optional. The Id of the artist to get, taken from the route.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetArtistQuery"/>.</returns>
    public GetArtistQuery Create(
        string? libraryId = null,
        string? artistId = null,
        bool includeLibraryId = true,
        bool includeArtistId = true)
    {
        return new GetArtistQuery(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetArtistQuery"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetArtistQuery"/> instances.</returns>
    public List<GetArtistQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
