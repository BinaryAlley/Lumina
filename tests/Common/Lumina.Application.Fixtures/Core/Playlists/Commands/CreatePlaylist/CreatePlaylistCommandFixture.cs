#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Playlists.Commands.CreatePlaylist;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Playlists.Commands.CreatePlaylist;

/// <summary>
/// Fixture class for the <see cref="CreatePlaylistCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreatePlaylistCommandFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="CreatePlaylistCommand"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the playlist.</param>
    /// <param name="description">Optional. The optional description of the playlist.</param>
    /// <param name="type">Optional. The kind of media the playlist holds. Defaults to the only supported kind.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeType">Whether the media type should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="CreatePlaylistCommand"/>.</returns>
    public CreatePlaylistCommand Create(
        string? name = null,
        string? description = null,
        PlaylistType? type = null,
        bool includeDescription = true,
        bool includeType = true)
    {
        return new CreatePlaylistCommand(
            name ?? _faker.Lorem.Sentence(3),
            includeDescription ? (description ?? _faker.Lorem.Sentence()) : null,
            includeType ? (type ?? PlaylistType.Music) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="CreatePlaylistCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="CreatePlaylistCommand"/> instances.</returns>
    public List<CreatePlaylistCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
