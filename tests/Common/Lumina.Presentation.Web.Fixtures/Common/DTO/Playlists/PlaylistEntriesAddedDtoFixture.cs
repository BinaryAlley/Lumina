#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistEntriesAddedDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntriesAddedDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="PlaylistEntriesAddedDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="playlistId">Optional Id of the playlist the entries were added to.</param>
    /// <param name="addedCount">Optional number of entries that were appended.</param>
    /// <param name="updatedOnUtc">Optional date and time when the playlist was last updated.</param>
    /// <param name="includeUpdatedOnUtc">Whether the last update date should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="PlaylistEntriesAddedDto"/> instance.</returns>
    public PlaylistEntriesAddedDto Create(
        Guid? playlistId = null,
        int? addedCount = null,
        DateTime? updatedOnUtc = null,
        bool includeUpdatedOnUtc = true)
    {
        DateTime resolvedUpdatedOnUtc = DateTime.SpecifyKind(updatedOnUtc ?? DateTime.UtcNow.AddHours(-_faker.Random.Int(1, 24)), DateTimeKind.Utc);
        return new PlaylistEntriesAddedDto(
            PlaylistId: playlistId ?? _faker.Random.Guid(),
            AddedCount: addedCount ?? _faker.Random.Int(0, 1_000),
            UpdatedOnUtc: includeUpdatedOnUtc ? resolvedUpdatedOnUtc : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="PlaylistEntriesAddedDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistEntriesAddedDto"/> instances.</returns>
    public List<PlaylistEntriesAddedDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
