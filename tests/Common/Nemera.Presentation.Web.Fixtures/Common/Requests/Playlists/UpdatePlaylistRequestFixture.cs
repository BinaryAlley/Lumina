#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Playlists;

/// <summary>
/// Fixture class for the <see cref="UpdatePlaylistRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaylistRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="UpdatePlaylistRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="name">Optional name of the playlist.</param>
    /// <param name="description">Optional description of the playlist.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="UpdatePlaylistRequest"/> instance.</returns>
    public UpdatePlaylistRequest Create(
        string? name = null,
        string? description = null,
        bool includeName = true,
        bool includeDescription = true)
    {
        return new UpdatePlaylistRequest(
            Name: includeName ? (name ?? _faker.Lorem.Sentence(3)) : null,
            Description: includeDescription ? (description ?? _faker.Lorem.Sentence(5)) : null);
    }

    /// <summary>
    /// Creates multiple <see cref="UpdatePlaylistRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UpdatePlaylistRequest"/> instances.</returns>
    public List<UpdatePlaylistRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
