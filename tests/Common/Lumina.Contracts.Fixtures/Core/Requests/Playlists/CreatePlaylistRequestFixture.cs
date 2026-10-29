#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Playlists;

/// <summary>
/// Fixture class for the <see cref="CreatePlaylistRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreatePlaylistRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="CreatePlaylistRequest"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the playlist.</param>
    /// <param name="description">Optional. The description of the playlist.</param>
    /// <param name="type">Optional. The kind of media the playlist holds. Defaults to the only supported kind.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeType">Whether the media type should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="CreatePlaylistRequest"/>.</returns>
    public CreatePlaylistRequest Create(
        string? name = null,
        string? description = null,
        PlaylistType? type = null,
        bool includeName = true,
        bool includeDescription = true,
        bool includeType = true)
    {
        return new CreatePlaylistRequest(
            includeName ? (name ?? _faker.Lorem.Sentence(3)) : null,
            includeDescription ? (description ?? _faker.Lorem.Sentence()) : null,
            includeType ? (type ?? PlaylistType.Music) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="CreatePlaylistRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="CreatePlaylistRequest"/> instances.</returns>
    public List<CreatePlaylistRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
