#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistDetailsResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistDetailsResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaylistDetailsResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the playlist.</param>
    /// <param name="name">Optional. The name of the playlist.</param>
    /// <param name="description">Optional. The description of the playlist.</param>
    /// <param name="type">Optional. The kind of media the playlist holds.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the playlist was last updated.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update time should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="PlaylistDetailsResponse"/>.</returns>
    public PlaylistDetailsResponse Create(
        Guid? id = null,
        string? name = null,
        string? description = null,
        PlaylistType? type = null,
        DateTime? updatedOnUtc = null,
        bool includeDescription = true,
        bool includeUpdatedOnUtc = true)
    {
        return new PlaylistDetailsResponse(
            id ?? _faker.Random.Guid(),
            name ?? _faker.Lorem.Sentence(3),
            includeDescription ? (description ?? _faker.Lorem.Sentence()) : null,
            type ?? PlaylistType.Music,
            includeUpdatedOnUtc ? (updatedOnUtc ?? DateTime.UtcNow.AddMinutes(-_faker.Random.Int(1, 1_440))) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="PlaylistDetailsResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistDetailsResponse"/> instances.</returns>
    public List<PlaylistDetailsResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
