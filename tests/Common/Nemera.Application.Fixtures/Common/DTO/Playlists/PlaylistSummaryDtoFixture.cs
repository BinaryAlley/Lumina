#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistSummaryDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistSummaryDtoFixture
{
    private const int MAXIMUM_ENTRY_COUNT = 10_000;

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaylistSummaryDto"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the playlist.</param>
    /// <param name="name">Optional. The name of the playlist.</param>
    /// <param name="description">Optional. The description of the playlist.</param>
    /// <param name="type">Optional. The kind of media the playlist holds. Defaults to the only supported kind.</param>
    /// <param name="entryCount">Optional. The number of entries of the playlist. Defaults to a bounded random count.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the playlist was created.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the playlist was last updated.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update date and time should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="PlaylistSummaryDto"/>.</returns>
    public PlaylistSummaryDto Create(
        Guid? id = null,
        string? name = null,
        string? description = null,
        PlaylistType? type = null,
        int? entryCount = null,
        DateTime? createdOnUtc = null,
        DateTime? updatedOnUtc = null,
        bool includeDescription = true,
        bool includeUpdatedOnUtc = false)
    {
        return new PlaylistSummaryDto(
            id ?? _faker.Random.Guid(),
            name ?? _faker.Lorem.Sentence(3),
            includeDescription ? (description ?? _faker.Lorem.Sentence()) : null,
            type ?? PlaylistType.Music,
            entryCount ?? _faker.Random.Int(0, MAXIMUM_ENTRY_COUNT),
            createdOnUtc ?? _faker.Date.Past(),
            includeUpdatedOnUtc ? (updatedOnUtc ?? _faker.Date.Recent()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="PlaylistSummaryDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistSummaryDto"/> instances.</returns>
    public List<PlaylistSummaryDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
