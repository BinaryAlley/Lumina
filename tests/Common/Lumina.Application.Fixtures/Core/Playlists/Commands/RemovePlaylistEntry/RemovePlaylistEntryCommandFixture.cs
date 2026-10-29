#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.RemovePlaylistEntry;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Playlists.Commands.RemovePlaylistEntry;

/// <summary>
/// Fixture class for the <see cref="RemovePlaylistEntryCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemovePlaylistEntryCommandFixture
{
    /// <summary>
    /// Creates a random valid <see cref="RemovePlaylistEntryCommand"/>.
    /// </summary>
    /// <param name="playlistId">Optional. The Id of the playlist, taken from the route.</param>
    /// <param name="playlistEntryId">Optional. The Id of the entry to remove, taken from the route.</param>
    /// <param name="includePlaylistId">Whether the playlist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePlaylistEntryId">Whether the entry Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="RemovePlaylistEntryCommand"/>.</returns>
    public RemovePlaylistEntryCommand Create(
        string? playlistId = null,
        string? playlistEntryId = null,
        bool includePlaylistId = true,
        bool includePlaylistEntryId = true)
    {
        return new RemovePlaylistEntryCommand(
            includePlaylistId ? (playlistId ?? Guid.NewGuid().ToString()) : null,
            includePlaylistEntryId ? (playlistEntryId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="RemovePlaylistEntryCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RemovePlaylistEntryCommand"/> instances.</returns>
    public List<RemovePlaylistEntryCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
