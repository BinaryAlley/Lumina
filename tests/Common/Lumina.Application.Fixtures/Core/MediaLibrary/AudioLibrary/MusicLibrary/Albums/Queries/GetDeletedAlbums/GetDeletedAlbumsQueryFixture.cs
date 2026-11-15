#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetDeletedAlbums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetDeletedAlbums;

/// <summary>
/// Fixture class for the <see cref="GetDeletedAlbumsQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedAlbumsQueryFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetDeletedAlbumsQuery"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose deleted albums are retrieved, taken from the route.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetDeletedAlbumsQuery"/>.</returns>
    public GetDeletedAlbumsQuery Create(
        string? libraryId = null,
        bool includeLibraryId = true)
    {
        return new GetDeletedAlbumsQuery(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetDeletedAlbumsQuery"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetDeletedAlbumsQuery"/> instances.</returns>
    public List<GetDeletedAlbumsQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
