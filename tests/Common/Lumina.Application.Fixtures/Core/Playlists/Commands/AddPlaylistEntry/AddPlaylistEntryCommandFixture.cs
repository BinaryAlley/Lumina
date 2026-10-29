#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.AddPlaylistEntry;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Playlists.Commands.AddPlaylistEntry;

/// <summary>
/// Fixture class for the <see cref="AddPlaylistEntryCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntryCommandFixture
{
    /// <summary>
    /// Creates a random valid <see cref="AddPlaylistEntryCommand"/>.
    /// </summary>
    /// <param name="playlistId">Optional. The Id of the playlist, taken from the route.</param>
    /// <param name="trackId">Optional. The Id of the track to add.</param>
    /// <param name="includePlaylistId">Whether the playlist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTrackId">Whether the track Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AddPlaylistEntryCommand"/>.</returns>
    public AddPlaylistEntryCommand Create(
        string? playlistId = null,
        Guid? trackId = null,
        bool includePlaylistId = true,
        bool includeTrackId = true)
    {
        return new AddPlaylistEntryCommand(
            includePlaylistId ? (playlistId ?? Guid.NewGuid().ToString()) : null,
            includeTrackId ? (trackId ?? Guid.NewGuid()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="AddPlaylistEntryCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AddPlaylistEntryCommand"/> instances.</returns>
    public List<AddPlaylistEntryCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
