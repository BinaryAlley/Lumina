#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Playlists;

/// <summary>
/// Fixture class for the <see cref="AddPlaylistScopeEntriesRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistScopeEntriesRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="AddPlaylistScopeEntriesRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="libraryId">Optional Id of the music library the scope belongs to.</param>
    /// <param name="artistId">Optional Id of the artist the scope is limited to.</param>
    /// <param name="albumId">Optional Id of the album the scope is limited to.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="AddPlaylistScopeEntriesRequest"/> instance.</returns>
    public AddPlaylistScopeEntriesRequest Create(
        Guid? libraryId = null,
        Guid? artistId = null,
        Guid? albumId = null,
        bool includeArtistId = true,
        bool includeAlbumId = true)
    {
        return new AddPlaylistScopeEntriesRequest(
            LibraryId: libraryId ?? Guid.NewGuid(),
            ArtistId: includeArtistId ? (artistId ?? Guid.NewGuid()) : null,
            AlbumId: includeAlbumId ? (albumId ?? Guid.NewGuid()) : null);
    }

    /// <summary>
    /// Creates multiple <see cref="AddPlaylistScopeEntriesRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AddPlaylistScopeEntriesRequest"/> instances.</returns>
    public List<AddPlaylistScopeEntriesRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
