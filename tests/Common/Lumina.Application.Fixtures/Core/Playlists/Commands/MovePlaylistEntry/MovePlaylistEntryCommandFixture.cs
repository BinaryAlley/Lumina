#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Playlists.Commands.MovePlaylistEntry;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Playlists.Commands.MovePlaylistEntry;

/// <summary>
/// Fixture class for the <see cref="MovePlaylistEntryCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovePlaylistEntryCommandFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MovePlaylistEntryCommand"/>.
    /// </summary>
    /// <param name="playlistId">Optional. The Id of the playlist, taken from the route.</param>
    /// <param name="playlistEntryId">Optional. The Id of the entry to move, taken from the route.</param>
    /// <param name="targetPosition">Optional. The new zero based position of the entry.</param>
    /// <param name="includePlaylistId">Whether the playlist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePlaylistEntryId">Whether the entry Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTargetPosition">Whether the target position should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="MovePlaylistEntryCommand"/>.</returns>
    public MovePlaylistEntryCommand Create(
        string? playlistId = null,
        string? playlistEntryId = null,
        int? targetPosition = null,
        bool includePlaylistId = true,
        bool includePlaylistEntryId = true,
        bool includeTargetPosition = true)
    {
        return new MovePlaylistEntryCommand(
            includePlaylistId ? (playlistId ?? Guid.NewGuid().ToString()) : null,
            includePlaylistEntryId ? (playlistEntryId ?? Guid.NewGuid().ToString()) : null,
            includeTargetPosition ? (targetPosition ?? _faker.Random.Int(0, 1_000)) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="MovePlaylistEntryCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MovePlaylistEntryCommand"/> instances.</returns>
    public List<MovePlaylistEntryCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
