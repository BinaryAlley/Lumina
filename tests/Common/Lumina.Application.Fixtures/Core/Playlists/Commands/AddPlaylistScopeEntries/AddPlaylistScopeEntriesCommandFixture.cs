#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.AddPlaylistScopeEntries;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Playlists.Commands.AddPlaylistScopeEntries;

/// <summary>
/// Fixture class for the <see cref="AddPlaylistScopeEntriesCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistScopeEntriesCommandFixture
{
    /// <summary>
    /// Creates a random valid <see cref="AddPlaylistScopeEntriesCommand"/>.
    /// </summary>
    /// <param name="playlistId">Optional. The Id of the playlist, taken from the route.</param>
    /// <param name="libraryId">Optional. The Id of the music library the scope belongs to.</param>
    /// <param name="artistId">Optional. The Id of the artist the scope is limited to.</param>
    /// <param name="albumId">Optional. The Id of the album the scope is limited to.</param>
    /// <param name="includePlaylistId">Whether the playlist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AddPlaylistScopeEntriesCommand"/>.</returns>
    public AddPlaylistScopeEntriesCommand Create(
        string? playlistId = null,
        Guid? libraryId = null,
        Guid? artistId = null,
        Guid? albumId = null,
        bool includePlaylistId = true,
        bool includeLibraryId = true,
        bool includeArtistId = false,
        bool includeAlbumId = false)
    {
        return new AddPlaylistScopeEntriesCommand(
            includePlaylistId ? (playlistId ?? Guid.NewGuid().ToString()) : null,
            includeLibraryId ? (libraryId ?? Guid.NewGuid()) : null,
            includeArtistId ? (artistId ?? Guid.NewGuid()) : null,
            includeAlbumId ? (albumId ?? Guid.NewGuid()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="AddPlaylistScopeEntriesCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AddPlaylistScopeEntriesCommand"/> instances.</returns>
    public List<AddPlaylistScopeEntriesCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
