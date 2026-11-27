#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Playlists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Playlists;

/// <summary>
/// Fixture class for the <see cref="UpdatePlaylistRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaylistRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UpdatePlaylistRequest"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the playlist.</param>
    /// <param name="description">Optional. The description of the playlist.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="UpdatePlaylistRequest"/>.</returns>
    public UpdatePlaylistRequest Create(
        string? name = null,
        string? description = null,
        bool includeName = true,
        bool includeDescription = true)
    {
        return new UpdatePlaylistRequest(
            includeName ? (name ?? _faker.Lorem.Sentence(3)) : null,
            includeDescription ? (description ?? _faker.Lorem.Sentence()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="UpdatePlaylistRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UpdatePlaylistRequest"/> instances.</returns>
    public List<UpdatePlaylistRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
