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
/// Fixture class for the <see cref="PlaylistSummaryResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistSummaryResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaylistSummaryResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the playlist.</param>
    /// <param name="name">Optional. The name of the playlist.</param>
    /// <param name="description">Optional. The description of the playlist.</param>
    /// <param name="type">Optional. The kind of media the playlist holds.</param>
    /// <param name="entryCount">Optional. The number of entries of the playlist.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the playlist was created.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the playlist was last updated.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update time should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="PlaylistSummaryResponse"/>.</returns>
    public PlaylistSummaryResponse Create(
        Guid? id = null,
        string? name = null,
        string? description = null,
        PlaylistType? type = null,
        int? entryCount = null,
        DateTime? createdOnUtc = null,
        DateTime? updatedOnUtc = null,
        bool includeDescription = true,
        bool includeUpdatedOnUtc = true)
    {
        return new PlaylistSummaryResponse(
            id ?? _faker.Random.Guid(),
            name ?? _faker.Lorem.Sentence(3),
            includeDescription ? (description ?? _faker.Lorem.Sentence()) : null,
            type ?? PlaylistType.Music,
            entryCount ?? _faker.Random.Int(0, 1_000),
            createdOnUtc ?? DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 30)),
            includeUpdatedOnUtc ? (updatedOnUtc ?? DateTime.UtcNow.AddMinutes(-_faker.Random.Int(1, 1_440))) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="PlaylistSummaryResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistSummaryResponse"/> instances.</returns>
    public List<PlaylistSummaryResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
