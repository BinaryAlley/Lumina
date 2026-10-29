#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistSummaryDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistSummaryDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="PlaylistSummaryDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional Id of the playlist.</param>
    /// <param name="name">Optional name of the playlist.</param>
    /// <param name="description">Optional description of the playlist.</param>
    /// <param name="type">Optional kind of media the playlist holds.</param>
    /// <param name="entryCount">Optional number of entries of the playlist.</param>
    /// <param name="createdOnUtc">Optional date and time when the playlist was created.</param>
    /// <param name="updatedOnUtc">Optional date and time when the playlist was last updated.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeUpdatedOnUtc">Whether the last update date should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="PlaylistSummaryDto"/> instance.</returns>
    public PlaylistSummaryDto Create(
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
        DateTime resolvedCreatedOnUtc = DateTime.SpecifyKind(createdOnUtc ?? DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 30)), DateTimeKind.Utc);
        DateTime resolvedUpdatedOnUtc = DateTime.SpecifyKind(updatedOnUtc ?? resolvedCreatedOnUtc.AddHours(_faker.Random.Int(1, 24)), DateTimeKind.Utc);
        return new PlaylistSummaryDto(
            Id: id ?? _faker.Random.Guid(),
            Name: name ?? _faker.Lorem.Sentence(3),
            Description: includeDescription ? (description ?? _faker.Lorem.Sentence(5)) : null,
            Type: type ?? PlaylistType.Music,
            EntryCount: entryCount ?? _faker.Random.Int(0, 1_000),
            CreatedOnUtc: resolvedCreatedOnUtc,
            UpdatedOnUtc: includeUpdatedOnUtc ? resolvedUpdatedOnUtc : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="PlaylistSummaryDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistSummaryDto"/> instances.</returns>
    public List<PlaylistSummaryDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
