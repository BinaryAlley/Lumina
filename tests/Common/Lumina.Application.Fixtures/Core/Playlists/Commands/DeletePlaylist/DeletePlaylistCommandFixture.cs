#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.DeletePlaylist;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Playlists.Commands.DeletePlaylist;

/// <summary>
/// Fixture class for the <see cref="DeletePlaylistCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletePlaylistCommandFixture
{
    /// <summary>
    /// Creates a random valid <see cref="DeletePlaylistCommand"/>.
    /// </summary>
    /// <param name="playlistId">Optional. The Id of the playlist, taken from the route.</param>
    /// <param name="includePlaylistId">Whether the playlist Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="DeletePlaylistCommand"/>.</returns>
    public DeletePlaylistCommand Create(
        string? playlistId = null,
        bool includePlaylistId = true)
    {
        return new DeletePlaylistCommand(
            includePlaylistId ? (playlistId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="DeletePlaylistCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeletePlaylistCommand"/> instances.</returns>
    public List<DeletePlaylistCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
