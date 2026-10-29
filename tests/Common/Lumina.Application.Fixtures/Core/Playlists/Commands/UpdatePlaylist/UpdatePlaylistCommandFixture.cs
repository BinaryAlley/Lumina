#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Playlists.Commands.UpdatePlaylist;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Playlists.Commands.UpdatePlaylist;

/// <summary>
/// Fixture class for the <see cref="UpdatePlaylistCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaylistCommandFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UpdatePlaylistCommand"/>.
    /// </summary>
    /// <param name="playlistId">Optional. The Id of the playlist, taken from the route.</param>
    /// <param name="name">Optional. The name of the playlist.</param>
    /// <param name="description">Optional. The description of the playlist.</param>
    /// <param name="includePlaylistId">Whether the playlist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="UpdatePlaylistCommand"/>.</returns>
    public UpdatePlaylistCommand Create(
        string? playlistId = null,
        string? name = null,
        string? description = null,
        bool includePlaylistId = true,
        bool includeName = true,
        bool includeDescription = true)
    {
        return new UpdatePlaylistCommand(
            includePlaylistId ? (playlistId ?? Guid.NewGuid().ToString()) : null,
            includeName ? (name ?? _faker.Lorem.Sentence(3)) : null,
            includeDescription ? (description ?? _faker.Lorem.Sentence()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="UpdatePlaylistCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UpdatePlaylistCommand"/> instances.</returns>
    public List<UpdatePlaylistCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
