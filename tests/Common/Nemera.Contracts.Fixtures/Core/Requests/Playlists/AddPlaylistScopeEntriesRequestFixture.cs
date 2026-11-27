#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Playlists;

/// <summary>
/// Fixture class for the <see cref="AddPlaylistScopeEntriesRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistScopeEntriesRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="AddPlaylistScopeEntriesRequest"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the music library the scope belongs to.</param>
    /// <param name="artistId">Optional. The Id of the artist the scope is limited to.</param>
    /// <param name="albumId">Optional. The Id of the album the scope is limited to.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AddPlaylistScopeEntriesRequest"/>.</returns>
    public AddPlaylistScopeEntriesRequest Create(
        Guid? libraryId = null,
        Guid? artistId = null,
        Guid? albumId = null,
        bool includeArtistId = false,
        bool includeAlbumId = false)
    {
        return new AddPlaylistScopeEntriesRequest(
            libraryId ?? Guid.NewGuid(),
            includeArtistId ? (artistId ?? Guid.NewGuid()) : null,
            includeAlbumId ? (albumId ?? Guid.NewGuid()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="AddPlaylistScopeEntriesRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AddPlaylistScopeEntriesRequest"/> instances.</returns>
    public List<AddPlaylistScopeEntriesRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
