#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Playlists;

/// <summary>
/// Fixture class for the <see cref="CreatePlaylistRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreatePlaylistRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="CreatePlaylistRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="name">Optional name of the playlist.</param>
    /// <param name="description">Optional description of the playlist.</param>
    /// <param name="type">Optional kind of media the playlist holds.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeType">Whether the media type should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="CreatePlaylistRequest"/> instance.</returns>
    public CreatePlaylistRequest Create(
        string? name = null,
        string? description = null,
        PlaylistType? type = null,
        bool includeName = true,
        bool includeDescription = true,
        bool includeType = true)
    {
        return new CreatePlaylistRequest(
            Name: includeName ? (name ?? _faker.Lorem.Sentence(3)) : null,
            Description: includeDescription ? (description ?? _faker.Lorem.Sentence(5)) : null,
            Type: includeType ? (type ?? PlaylistType.Music) : null);
    }

    /// <summary>
    /// Creates multiple <see cref="CreatePlaylistRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="CreatePlaylistRequest"/> instances.</returns>
    public List<CreatePlaylistRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
